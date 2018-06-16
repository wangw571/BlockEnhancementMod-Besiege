﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlockEnhancementMod.Blocks
{
    class CameraScript : EnhancementBlock
    {
        MToggle CameraLookAtToggle;
        MKey LockTargetKey;

        public bool cameraLookAtToggled = false;
        public int selfIndex;
        public Transform target;
        public Transform realCameraTransform;
        public List<KeyCode> lockKeys = new List<KeyCode> { KeyCode.Delete };

        protected override void SafeAwake()
        {

            CameraLookAtToggle = AddToggle("追踪摄像机", "TrackingCamera", cameraLookAtToggled);
            CameraLookAtToggle.Toggled += (bool value) => { cameraLookAtToggled = LockTargetKey.DisplayInMapper = value; ChangedProperties(); };
            BlockDataLoadEvent += (XDataHolder BlockData) => { cameraLookAtToggled = CameraLookAtToggle.IsActive; };

            LockTargetKey = AddKey("锁定目标", "lockTarget", lockKeys);
            LockTargetKey.KeysChanged += ChangedProperties;

            if (!Machine.Active().gameObject.GetComponent<CameraCompositeTrackerScript>())
            {
                Machine.Active().gameObject.AddComponent<CameraCompositeTrackerScript>();
            }



            // Get the actual camera's transform, not the joint's transform
            realCameraTransform = GetComponent<FixedCameraBlock>().CompoundTracker;
            // Add reference to the camera's buildindex
            selfIndex = GetComponent<BlockBehaviour>().BuildIndex;
#if DEBUG
            ConsoleController.ShowMessage("摄像机添加进阶属性");
#endif

        }

        public override void DisplayInMapper(bool value)
        {
            CameraLookAtToggle.DisplayInMapper = value;
            LockTargetKey.DisplayInMapper = value && cameraLookAtToggled;
        }

        public override void LoadConfiguration(XDataHolder BlockData)
        {
            if (BlockData.HasKey("bmt-" + "CameraTarget"))
            {
                Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Add(selfIndex, BlockData.ReadInt("bmt-" + "CameraTarget"));
            }
        }

        public override void SaveConfiguration(XDataHolder BlockData)
        {
            if (Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.ContainsKey(selfIndex))
            {
                BlockData.Write("bmt-" + "CameraTarget", Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic[selfIndex]);
            }
        }

        protected override void OnSimulateStart()
        {
            // Trying to read previously saved target
            int targetIndex = -1;
            BlockBehaviour targetBlock = new BlockBehaviour();
            BlockBehaviour simBlock = new BlockBehaviour();
            // Read the target's buildIndex
            try
            {
                Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.TryGetValue(selfIndex, out targetIndex);
            }
            catch (Exception)
            {
                ConsoleController.ShowMessage("Cannot get target index");
            }
            // Aquire target block from the target's index
            try
            {
                Machine.Active().GetBlockFromIndex(targetIndex, out targetBlock);
                simBlock = Machine.Active().GetSimBlock(targetBlock);
            }
            catch (Exception)
            {
                ConsoleController.ShowMessage("Cannot get target block");
            }
            // Aquire target's transform
            try
            {
                target = simBlock.transform;
            }
            catch (Exception)
            {
                ConsoleController.ShowMessage("Cannot get target transform");
            }
        }

        protected override void OnSimulateFixedUpdate()
        {
            if (cameraLookAtToggled && LockTargetKey.IsReleased)
            {
                // Aquire the target to look at
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    target = hit.transform;
                    // Trying to save target's buildIndex to the dictionary
                    // If not a machine block, set targetIndex to -1
                    int targetIndex = -1;
                    try
                    {
                        targetIndex = target.GetComponent<BlockBehaviour>().BuildIndex;
                    }
                    catch (Exception)
                    {
                        ConsoleController.ShowMessage("Not a machine block");
                    }
                    if (targetIndex != -1)
                    {
                        // Make sure the dupicated key exception is handled
                        try
                        {
                            Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Add(selfIndex, target.GetComponent<BlockBehaviour>().BuildIndex);
                        }
                        catch (Exception)
                        {
                            // Remove the old record, then add the new record
                            Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Remove(selfIndex);
                            Machine.Active().GetComponent<CameraCompositeTrackerScript>().previousTargetDic.Add(selfIndex, target.GetComponent<BlockBehaviour>().BuildIndex);
                        }
                    }
                }

            }
        }

        protected override void LateUpdate()
        {
            if (cameraLookAtToggled && target != null && StatMaster.levelSimulating)
            {
                // Keep the camera focusing on the target
                Vector3 positionDiff = target.position - realCameraTransform.position;
                Vector3 rotatingAxis = (realCameraTransform.up - Vector3.Dot(positionDiff, realCameraTransform.up) * positionDiff).normalized;
                realCameraTransform.LookAt(target);
            }
        }
    }

    class CameraCompositeTrackerScript : MonoBehaviour
    {
        public Dictionary<int, int> previousTargetDic = new Dictionary<int, int>();
    }
}