using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityAssimp {
    public class IBone {
        public int BoneIndex;
        public float Weight;
        public IBone() {
            BoneIndex = 0;
            Weight = 0;
        }
    }

    public class BoneVertex {
        public IBone[] bones;
        public int numbones;
        public BoneVertex() {
            bones = new IBone[4];
            for (int i = 0; i < 4; i++) {
                bones[i] = new IBone();
            }
            numbones = 0;
        }

        public void addBone(int bone, float w) {
            for (int i = 0; i < 4; i++) {
                if (bones[i].Weight == 0) {
                    bones[i].BoneIndex = bone;
                    bones[i].Weight = w;
                    numbones++;
                    return;
                }
            }
        }
    }

    public class AssimpJoint {
        public string parentName;
        public string Name;
        public string Path;
        public Vector3 Position;
        public Quaternion Orientation;
        public AssimpJoint parent;
        public Transform transform;

        public AssimpJoint() {
            Name = "";
            parentName = "";
            parent = null;
            Path = "";
        }
    }
}