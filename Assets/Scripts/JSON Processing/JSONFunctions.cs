using SimpleFileBrowser;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using UnityEngine;


namespace JSONFunctions
{
    public class JSONFileFunctions : MonoBehaviour
    {
        [Header("Script Connections")]
        public static ImageReferencing imageReference;
        public static GameStateManager stateManager;

        public class GameData
        {
            public string filepath;
            public List<PieceData> game;
        }
        public PieceData piecedata;
        public static string FilePath;
        //public InteractionManager interactionManager;
            void Start()
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Files", ".json"));
            FileBrowser.SetDefaultFilter(".json");
        }

        public static void CreateOrEditJSON(string filepath)
        {
            try
            {
                var gamedata = new GameData();
                gamedata.game = new List<PieceData>();
                int i = 0;
                string folder = Path.GetDirectoryName(filepath);
                string JSONFilePath = folder + @"\" + DateTime.Today.ToString("d").Replace("/", "_");
                string altfilepath = JSONFilePath;
                var data = new List<PieceData>();
                var pieces = GameObject.FindGameObjectsWithTag("Piece");

                foreach (GameObject piece in pieces)
                {
                    piece.tag = "Piece";

                    var script = piece.GetComponent<PuzzlePiece>();

                    if (script != null)
                    {
                        script.UpdatePosition();

                        if (script.Data != null)
                        {
                            Transform root = InteractionManager.GetRoot(piece.transform);

                           
                            var a = new PieceData
                            {
                               Id = script.Data.Id,
                               Row = script.Data.Row,
                               Column = script.Data.Column,
                               GroupId = script.Data.GroupId,
                               TopEdge = script.Data.TopEdge,
                               RightEdge = script.Data.RightEdge,
                               BottomEdge = script.Data.BottomEdge,
                               LeftEdge = script.Data.LeftEdge,
                               Position = root.position,
                               Rotation = root.eulerAngles.z,
                            };
                            data.Add(a);
                            
                        }
                    }
                }
                gamedata.game = data;
                gamedata.filepath = filepath;
                string json = JsonUtility.ToJson(gamedata);

                if (File.Exists(JSONFilePath+".json"))
                {
                    while (File.Exists(altfilepath + ".json"))
                    {
                        altfilepath = JSONFilePath + $"_{i++}";
                    }
                    File.WriteAllText(altfilepath + ".json", json);
                }
                else
                {
                    File.WriteAllText(JSONFilePath + ".json", json);
                }
               

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateOrEditJSON: " + ex.Message);
            }
        }

        public static GameData ReadJSON(string filepath)
        {
            FilePath = filepath;
            try
            {
                string json = File.ReadAllText(filepath);
                return JsonUtility.FromJson <GameData> (json);

               
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in ReadJSON:" + ex.Message);
                return null;
            }
        }
        public static void OpenJSONBrowser()
        {
            Debug.Log("FILE 1");
            FileBrowser.ShowLoadDialog((paths) => { OnFileSelected(paths[0]); },
                () => { Debug.Log("File selection cancelled."); },
                FileBrowser.PickMode.Files, false, null, null, "Select Previous Game", "Load"
            );
            Debug.Log("FILE 2");
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Files", ".json"));
            FileBrowser.SetDefaultFilter(".json");
            Debug.Log("FILE 4");
        }
        private static void OnFileSelected(string path)
        {
            Debug.Log(path);
            Debug.Log("FILE 3");
            var data = ReadJSON(path);
            Debug.Log("Read JSON");
            var pieces = data.game;
            var Texture = data.filepath;
            Debug.Log(Texture);
            byte[] fileData = File.ReadAllBytes(Texture);
            Debug.Log("bytes done");
            Texture2D texture = new Texture2D(2, 2);
            
            Debug.Log("Got Texture");
            if (texture.LoadImage(fileData))
            {
                
                Debug.Log("Loaded Texture");
                imageReference.imageReference.gameObject.SetActive(true);
                Debug.Log("set active");
                JSONFileFunctions.stateManager.image = texture;
                if (imageReference != null)
                {
                    Debug.Log("ref not null");
                    imageReference.UpdateImages();
                }
                Debug.Log("Update Image");
                Debug.Log("FILE 5");
                stateManager.PrepareNewGame(texture);
                Debug.Log("FILE 5");
                //stateManager.GeneratePuzzleFromJSON(texture, pieces);
                Debug.Log("FILE 5");
            }
        }
    }
}
