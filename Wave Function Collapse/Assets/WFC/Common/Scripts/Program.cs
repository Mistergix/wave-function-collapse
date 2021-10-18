using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace WFC
{
    public class Program : MonoBehaviour
    {
        [SerializeField] private List<WFCParams> wfcParams;
        [SerializeField] private string outputRelativePath = "WFC/Outputs"; 
        void Start()
        {
            var random = new Random();
            foreach (var wfcParam in wfcParams)
            {
                var model = wfcParam.GetModel();
                for (var i = 0; i < wfcParam.Screenshots; i++)
                {
                    var seed = random.Next();
                    var success = model.Run(seed, wfcParam.Limit);
                    if (success)
                    {
                        var texture = model.GetGraphics();
                        var sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height),
                            new Vector2(0.5f, 0.5f), 100.0f);
                        SaveSpriteAsAsset(sprite, $"{outputRelativePath}/{name} {seed}.png");
                    }
                    else
                    {
                        Debug.Log("CONTRADICTION");
                    }
                }
            }
        }
        
        // proj_path should be relative to the Assets folder.
        static Sprite SaveSpriteAsAsset(Sprite sprite, string proj_path)
        {
            var abs_path = Path.Combine(Application.dataPath, proj_path);
            proj_path = Path.Combine("Assets", proj_path);
 
            Directory.CreateDirectory(Path.GetDirectoryName(abs_path) ?? string.Empty);
            File.WriteAllBytes(abs_path, ImageConversion.EncodeToPNG(sprite.texture));
 
            AssetDatabase.Refresh();
 
            var ti = AssetImporter.GetAtPath(proj_path) as TextureImporter;
            ti.spritePixelsPerUnit = sprite.pixelsPerUnit;
            ti.mipmapEnabled = false;
            ti.textureType = TextureImporterType.Sprite;
 
            EditorUtility.SetDirty(ti);
            ti.SaveAndReimport();
 
            return AssetDatabase.LoadAssetAtPath<Sprite>(proj_path);
        }
    }
}

