using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace UnityAssimp {
    public class Utils {

        public static Texture2D loadTex(string texturename) {

            Texture2D tex = null;
            if (File.Exists(texturename)) {
                tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texturename, typeof(Texture2D));
            } else if (File.Exists(texturename + ".PNG")) {
                tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texturename + ".PNG", typeof(Texture2D));
            } else if (File.Exists(texturename + ".JPG")) {
                tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texturename + ".JPG", typeof(Texture2D));
            } else if (File.Exists(texturename + ".BMP")) {
                tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texturename + ".BMP", typeof(Texture2D));
            } else if (File.Exists(texturename + ".TGA")) {
                tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texturename + ".TGA", typeof(Texture2D));
            } else if (File.Exists(texturename + ".DDS")) {
                tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texturename + ".DDS", typeof(Texture2D));
            }
            return tex;
        }
    }
}