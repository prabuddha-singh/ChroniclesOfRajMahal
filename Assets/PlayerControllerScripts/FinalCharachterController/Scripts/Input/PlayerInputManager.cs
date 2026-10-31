using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PrabuddhaSingh.FinalCharachterController{

    [DefaultExecutionOrder(-3)]
   public class PlayerInputManager : MonoBehaviour
  {
        public static PlayerInputManager Instance ;

        public PlayerControls PlayerControls { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
        }

        private void OnEnable()
        {
            Debug.Log("Player input manager Enabled");
            PlayerControls = new PlayerControls();
            PlayerControls.Enable();
        }

        private void OnDisable()
        {
            Debug.LogError(
        "PLAYER INPUT MANAGER DISABLED!\n" +
        "Scene: " + gameObject.scene.name + "\n" +
        "Stack Trace:\n" + Environment.StackTrace
    );
            Debug.Log("Player input manager Disabled");
            if(PlayerControls != null)
            {
                PlayerControls.Disable();
                PlayerControls.Dispose();
                PlayerControls=null;
            }           
        }
  }
}