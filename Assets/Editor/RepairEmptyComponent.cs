using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;

static class RepairEmptyComponent
{
    const string PreferredFallbackMaterialPath = "Assets/Materials/BoxMaterial.mat";

    [MenuItem("Tools/ProBuilder/Repair/Rebuild And Restore Materials", false)]
    public static void RebuildAndRestoreMaterials()
    {
        var fallbackMaterial = AssetDatabase.LoadAssetAtPath<Material>(PreferredFallbackMaterialPath);
        var meshes = UObject.FindObjectsByType<ProBuilderMesh>(FindObjectsSortMode.None);
        var report = new StringBuilder();

        int rebuilt = 0;
        int materialFixed = 0;
        int skipped = 0;

        try
        {
            for (int i = 0; i < meshes.Length; i++)
            {
                var mesh = meshes[i];
                EditorUtility.DisplayProgressBar(
                    "Repair ProBuilder Objects",
                    "Repairing " + mesh.name,
                    meshes.Length == 0 ? 1f : (float)i / meshes.Length);

                if (mesh == null)
                    continue;

                var meshFilter = mesh.GetComponent<MeshFilter>();
                var renderer = mesh.GetComponent<MeshRenderer>();
                var sourceMesh = GetSourceMesh(mesh, meshFilter);

                if ((sourceMesh == null || sourceMesh.vertexCount == 0) && mesh.vertexCount < 1)
                {
                    skipped++;
                    report.AppendLine("Skipped " + mesh.name + ": no MeshFilter, ProBuilder m_Mesh, or MeshCollider mesh data was available.");
                    continue;
                }

                var material = GetRepairMaterial(renderer, fallbackMaterial);

                Undo.RegisterFullObjectHierarchyUndo(mesh.gameObject, "Repair ProBuilder Mesh");

                if (meshFilter == null)
                    meshFilter = Undo.AddComponent<MeshFilter>(mesh.gameObject);

                if (renderer == null)
                    renderer = Undo.AddComponent<MeshRenderer>(mesh.gameObject);

                if (sourceMesh != null && sourceMesh.vertexCount > 0)
                    meshFilter.sharedMesh = sourceMesh;

                if (renderer != null && MaterialSlotsNeedRepair(renderer.sharedMaterials, material))
                {
                    renderer.sharedMaterials = new[] { material };
                    materialFixed++;
                }

                bool needsImport = mesh.vertexCount < 1 || mesh.faces == null || mesh.faces.Count < 1;
                if (needsImport && sourceMesh != null && sourceMesh.vertexCount > 0)
                {
                    var importer = new MeshImporter(mesh.gameObject);
                    importer.Import(new MeshImportSettings
                    {
                        quads = true,
                        smoothing = true,
                        smoothingAngle = 1f
                    });
                    rebuilt++;
                }

                if (material != null && mesh.faces != null && mesh.faces.Count > 0)
                {
                    mesh.SetMaterial(mesh.faces, material);
                    materialFixed++;
                }

                mesh.ToMesh();
                mesh.Refresh();

                EditorUtility.SetDirty(mesh);
                if (renderer != null)
                    EditorUtility.SetDirty(renderer);
                if (meshFilter != null)
                    EditorUtility.SetDirty(meshFilter);
            }
        }
        catch (Exception e)
        {
            report.AppendLine(e.ToString());
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        var message = "Checked " + meshes.Length + " ProBuilder object(s).\n"
            + "Rebuilt: " + rebuilt + "\n"
            + "Material repairs: " + materialFixed + "\n"
            + "Skipped: " + skipped;

        if (report.Length > 0)
            Debug.LogWarning(report.ToString());

        Debug.Log(message);
        EditorUtility.DisplayDialog("Repair ProBuilder Objects", message, "OK");
    }

    [MenuItem("Tools/ProBuilder/Repair/Rebuild Empty ProBuilderMesh Components", false)]
    public static void MenuForceSceneRefresh()
    {
        RebuildAndRestoreMaterials();
    }

    static Material GetRepairMaterial(MeshRenderer renderer, Material fallbackMaterial)
    {
        if (renderer != null)
        {
            var existing = renderer.sharedMaterials.FirstOrDefault(IsUsableMaterial);
            if (existing != null)
                return existing;
        }

        if (fallbackMaterial != null)
            return fallbackMaterial;

        return BuiltinMaterials.defaultMaterial;
    }

    static Mesh GetSourceMesh(ProBuilderMesh mesh, MeshFilter meshFilter)
    {
        var source = meshFilter == null ? null : meshFilter.sharedMesh;
        if (HasVertices(source))
            return source;

        source = GetSerializedMesh(mesh, "m_Mesh");
        if (HasVertices(source))
            return source;

        if (meshFilter != null)
        {
            source = GetSerializedMesh(meshFilter, "m_Mesh");
            if (HasVertices(source))
                return source;
        }

        var meshCollider = mesh.GetComponent<MeshCollider>();
        source = meshCollider == null ? null : meshCollider.sharedMesh;
        return HasVertices(source) ? source : null;
    }

    static Mesh GetSerializedMesh(UObject target, string propertyName)
    {
        if (target == null)
            return null;

        var serializedObject = new SerializedObject(target);
        var property = serializedObject.FindProperty(propertyName);
        return property == null ? null : property.objectReferenceValue as Mesh;
    }

    static bool HasVertices(Mesh mesh)
    {
        return mesh != null && mesh.vertexCount > 0;
    }

    static bool MaterialSlotsNeedRepair(Material[] materials, Material repairMaterial)
    {
        return repairMaterial != null
            && (materials == null || materials.Length == 0 || materials.Any(material => material == null));
    }

    static bool IsUsableMaterial(Material material)
    {
        return material != null
            && material.shader != null
            && material.shader.isSupported
            && !material.name.Contains("InvisibleFace")
            && !material.name.Contains("NoDraw");
    }
}
