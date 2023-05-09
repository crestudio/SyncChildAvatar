#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

using VRC.SDKBase;

/*
 * VRSuya Sync Child Avatar
 * Contact : vrsuya@gmail.com // Twitter : https://twitter.com/VRSuya
 * Forked from curlune/VRCUtil ( https://github.com/curlune/VRCUtil )
 * Thanks to Dalgona.
 */

namespace VRSuya.SyncChildAvatar {

	public class SyncChildAvatar : EditorWindow {

		[MenuItem("Tools/VRSuya/Sync All Child Avatar Bone")]
		public static void SyncAllChildAvatar() {
            List<HumanBodyBones> HumanBodyBoneList = GetHumanBoneList();
            List<VRC_AvatarDescriptor> VRCAvatars = GetVRCAvatarList();
			foreach (var VRCAvatar in VRCAvatars) {
                Animator ParentAvatarAnimator = VRCAvatar.GetComponent<Animator>();
				if (!ParentAvatarAnimator) continue;
                Animator[] ChildAvatarAnimator = VRCAvatar.GetComponentsInChildren<Animator>();
				foreach (var ChildAnimator in ChildAvatarAnimator) {
					CreateConstraintComponents(ParentAvatarAnimator, ChildAnimator, HumanBodyBoneList);
				}
			}
			Debug.Log("[VRSuya SyncChildAvatar] Synced All Child Avatars");
		}

		// VRC 아바타 리스트 반환
		private static List<VRC_AvatarDescriptor> GetVRCAvatarList() {
			List<VRC_AvatarDescriptor> AllVRCAvatars = VRC.Tools.FindSceneObjectsOfTypeAll<VRC_AvatarDescriptor>().ToList();
            List<VRC_AvatarDescriptor> VRCAvatars = AllVRCAvatars.Where(Avatar => Avatar.gameObject.activeInHierarchy).ToList();
			return VRCAvatars;
		}

		// 아마추어 휴머노이드 본 리스트 반환
		private static List<HumanBodyBones> GetHumanBoneList() {
			return Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>().ToList();
		}

		// Rotation Constraint 및 Position Constraint 생성 및 연결
		private static void CreateConstraintComponents(Animator ParentAnimator, Animator ChildAnimator, List<HumanBodyBones> HumanBodyBoneList) {
			if (ParentAnimator == ChildAnimator) return;
			foreach (HumanBodyBones Bone in HumanBodyBoneList) {
				if (Bone == HumanBodyBones.LastBone) continue;
                Transform ParentBoneTransform = ParentAnimator.GetBoneTransform(Bone);
                Transform ChildBoneTransform = ChildAnimator.GetBoneTransform(Bone);
				if (!ParentBoneTransform || !ChildBoneTransform) continue;

                RotationConstraint TargetRotationConstraint = GetOrCreateComponent<RotationConstraint>(ChildBoneTransform.gameObject);
				TargetRotationConstraint.AddSource(new ConstraintSource() { sourceTransform = ParentBoneTransform, weight = 1.0f });
				TargetRotationConstraint.constraintActive = true;

				if (Bone == HumanBodyBones.Hips) {
                    PositionConstraint TargetPositionConstraint = GetOrCreateComponent<PositionConstraint>(ChildBoneTransform.gameObject);
					TargetPositionConstraint.AddSource(new ConstraintSource() { sourceTransform = ParentBoneTransform, weight = 1.0f });
					TargetPositionConstraint.constraintActive = true;
				}
			}
		}

		// 컴포넌트 검사 메소드
		private static TargetComponent GetOrCreateComponent<TargetComponent>(GameObject TargetGameObject) where TargetComponent : Component {
            TargetComponent Component = TargetGameObject.GetComponent<TargetComponent>();
			if (!Component) Component = TargetGameObject.AddComponent<TargetComponent>();
			return Component;
		}
	}
}
#endif