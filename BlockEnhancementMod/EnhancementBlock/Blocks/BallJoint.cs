﻿using Modding;
using Modding.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlockEnhancementMod
{
    class BallJointScript : EnhancementBlock
    {

        public MToggle RotationToggle;
        public bool Rotation = false;
        private bool orginRotation = false;

        private ConfigurableJoint CJ;
        private float orginBreakTorque = 18000;

        public override void SafeAwake()
        {

            RotationToggle = BB.AddToggle(LanguageManager.cvJoint, "Rotation", Rotation);
            RotationToggle.Toggled += (bool value) => { Rotation = value; ChangedProperties(); };


#if DEBUG
            ConsoleController.ShowMessage("球铰添加进阶属性");
#endif
        }

        public override void DisplayInMapper(bool value)
        {
            RotationToggle.DisplayInMapper = value;
        }

        public override void ChangeParameter()
        {
            CJ = GetComponent<ConfigurableJoint>();

            if (!EnhancementEnabled) { Rotation = orginRotation; }

            if (Rotation)
            {
                CJ.angularYMotion = ConfigurableJointMotion.Locked;
                CJ.breakTorque = Mathf.Infinity;
            }
            else
            {
                CJ.angularYMotion = ConfigurableJointMotion.Free;
                CJ.breakTorque = orginBreakTorque;
            }
        }
    }

}
