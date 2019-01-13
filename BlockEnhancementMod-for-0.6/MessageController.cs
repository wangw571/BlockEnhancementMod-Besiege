﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Modding;
using Modding.Blocks;
using Modding.Levels;
using BlockEnhancementMod.Blocks;

namespace BlockEnhancementMod
{
    public static class Messages
    {
        //For rockets
        public static MessageType rocketTargetBlockBehaviourMsg;
        public static MessageType rocketTargetEntityMsg;
        public static MessageType rocketTargetNullMsg;
        public static MessageType rocketFiredMsg;
        public static MessageType rocketRayToHostMsg;
        public static MessageType rocketHighExploPosition;
        public static MessageType rocketLockOnMeMsg;
        public static MessageType rocketLostTargetMsg;
    }

    public class MessageController : SingleInstance<MessageController>
    {

        public override string Name { get; } = "Message Controller";

        public MessageController()
        {
            //Initiating messages
            Messages.rocketFiredMsg = ModNetworking.CreateMessageType(DataType.Block);
            Messages.rocketTargetBlockBehaviourMsg = ModNetworking.CreateMessageType(DataType.Block, DataType.Block);
            Messages.rocketTargetEntityMsg = ModNetworking.CreateMessageType(DataType.Entity, DataType.Block);
            Messages.rocketTargetNullMsg = ModNetworking.CreateMessageType(DataType.Block);
            Messages.rocketRayToHostMsg = ModNetworking.CreateMessageType(DataType.Vector3, DataType.Vector3, DataType.Block);
            Messages.rocketHighExploPosition = ModNetworking.CreateMessageType(DataType.Vector3, DataType.Single);
            Messages.rocketLockOnMeMsg = ModNetworking.CreateMessageType(DataType.Block, DataType.Integer);
            Messages.rocketLostTargetMsg = ModNetworking.CreateMessageType(DataType.Block);

            //Initiating callbacks
            ModNetworking.Callbacks[Messages.rocketHighExploPosition] += (Message msg) =>
            {
                if (StatMaster.isClient)
                {
                    Vector3 position = (Vector3)msg.GetData(0);
                    float bombExplosiveCharge = (float)msg.GetData(1);
                    int levelBombCategory = 4;
                    int levelBombID = 5001;
                    float radius = 7f;
                    float power = 3600f;
                    float torquePower = 100000f;
                    float upPower = 0.25f;
                    try
                    {
                        GameObject bomb = UnityEngine.Object.Instantiate(PrefabMaster.LevelPrefabs[levelBombCategory].GetValue(levelBombID).gameObject);
                        bomb.transform.position = position;
                        ExplodeOnCollide bombControl = bomb.GetComponent<ExplodeOnCollide>();
                        bomb.transform.localScale = Vector3.one * bombExplosiveCharge;
                        bombControl.radius = radius * bombExplosiveCharge;
                        bombControl.power = power * bombExplosiveCharge;
                        bombControl.torquePower = torquePower * bombExplosiveCharge;
                        bombControl.upPower = upPower;
                        bombControl.Explodey();
                    }
                    catch { }
                }
            };

            ModNetworking.Callbacks[Messages.rocketFiredMsg] += (Message msg) =>
            {
                Block rocketBlock = (Block)msg.GetData(0);
                TimedRocket rocket = rocketBlock.GameObject.GetComponent<TimedRocket>();
                RocketsController.Instance.UpdateRocketFiredStatus(rocket);
            };

            ModNetworking.Callbacks[Messages.rocketTargetBlockBehaviourMsg] += (Message msg) =>
            {
#if DEBUG
                Debug.Log("Receive block target");
#endif
                Block rocketBlock = (Block)msg.GetData(1);
                RocketScript rocket = rocketBlock.GameObject.GetComponent<RocketScript>();
                rocket.target = ((Block)msg.GetData(0)).GameObject.transform;
                rocket.targetCollider = rocket.target.gameObject.GetComponentInChildren<Collider>(true);

            };

            ModNetworking.Callbacks[Messages.rocketTargetEntityMsg] += (Message msg) =>
            {
#if DEBUG
                Debug.Log("Receive entity target");
#endif
                Block rocketBlock = (Block)msg.GetData(1);
                RocketScript rocket = rocketBlock.GameObject.GetComponent<RocketScript>();
                rocket.target = ((Entity)msg.GetData(0)).GameObject.transform;
                rocket.targetCollider = rocket.target.gameObject.GetComponentInChildren<Collider>(true);
            };

            ModNetworking.Callbacks[Messages.rocketTargetNullMsg] += (Message msg) =>
            {
#if DEBUG
                Debug.Log("Receive entity target");
#endif
                Block rocketBlock = (Block)msg.GetData(0);
                RocketScript rocket = rocketBlock.GameObject.GetComponent<RocketScript>();
                rocket.target = null;
                rocket.targetCollider = null;

            };

            ModNetworking.Callbacks[Messages.rocketRayToHostMsg] += (Message msg) =>
            {
                Block rocketBlock = (Block)msg.GetData(2);
                RocketScript rocket = rocketBlock.GameObject.GetComponent<RocketScript>();
                rocket.rayFromClient = new Ray((Vector3)msg.GetData(0), (Vector3)msg.GetData(1));
                rocket.activeGuide = false;
                rocket.receivedRayFromClient = true;
            };

            ModNetworking.Callbacks[Messages.rocketLockOnMeMsg] += (Message msg) =>
            {
                Block rocket = (Block)msg.GetData(0);
                int targetMachineID = (int)msg.GetData(1);
                RocketsController.Instance.UpdateRocketTarget(rocket.InternalObject, targetMachineID);

            };
            ModNetworking.Callbacks[Messages.rocketLostTargetMsg] += (Message msg) =>
            {
                Block rocket = (Block)msg.GetData(0);
                RocketsController.Instance.RemoveRocketTarget(rocket.InternalObject);
            };
        }
    }
}
