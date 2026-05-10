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
        public static string CurrentSaveFilePath;

        [Serializable]
        public class GameData
        {
            public string filepath;
            public int rows;
            public int columns;
            public int generationSeed;
            public float elapsedTime;  // ← add this
            public List<PieceData> game;
        }
        public PieceData piecedata;
        public static string CurrentImagePath;
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
                string imageName = Path.GetFileNameWithoutExtension(filepath);

                string JSONFilePath =
                    folder + @"\" +
                    imageName + "_" +
                    DateTime.Now.ToString("yyyy-MM-dd");
                string altfilepath = JSONFilePath;
                var data = new List<PieceData>();
                var pieces = GameObject.FindGameObjectsWithTag("Piece");

                GameStateManager stateManager = GameObject.FindFirstObjectByType<GameStateManager>();

                if (stateManager != null)
                {
                    gamedata.rows = stateManager.currentRows;
                    gamedata.columns = stateManager.currentColumns;
                    gamedata.generationSeed = stateManager.currentGenerationSeed;
                    gamedata.elapsedTime = stateManager.elapsedTime;
                }

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
                                Position = piece.transform.position,
                                Rotation = piece.transform.eulerAngles.z,
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
                    CurrentSaveFilePath = altfilepath + ".json";
                    File.WriteAllText(CurrentSaveFilePath, json);
                }
                else
                {
                    CurrentSaveFilePath = JSONFilePath + ".json";
                    File.WriteAllText(CurrentSaveFilePath, json);
                }
               

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateOrEditJSON: " + ex.Message);
            }
        }

        public static GameData ReadJSON(string filepath)
        {
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
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Files", ".json"));
            FileBrowser.SetDefaultFilter(".json");

            FileBrowser.ShowLoadDialog(
                (paths) => { OnFileSelected(paths[0]); },
                () => { Debug.Log("File selection cancelled."); },
                FileBrowser.PickMode.Files, false, null, null, "Select Previous Game", "Load"
            );
        }
        private static void OnFileSelected(string path)
        {
            Debug.Log(path);
            Debug.Log("FILE 3");
            var data = ReadJSON(path);
            Debug.Log("Read JSON");
            var pieces = data.game;
            var Texture = data.filepath;
            CurrentImagePath = Texture;
            Debug.Log(Texture);
            byte[] fileData = File.ReadAllBytes(Texture);
            Debug.Log("bytes done");
            Texture2D texture = new Texture2D(2, 2);
            
            Debug.Log("Got Texture");
            if (texture.LoadImage(fileData))
            {
                
                Debug.Log("Loaded Texture");
                GameStateManager stateManager = GameObject.FindAnyObjectByType<GameStateManager>();
                ImageReferencing imageReference = GameObject.FindAnyObjectByType<ImageReferencing>();

                if (stateManager == null)
                {
                    Debug.LogError("GameStateManager not found.");
                    return;
                }

                stateManager.image = texture;
                if (imageReference != null)
                {
                    imageReference.imageReference.gameObject.SetActive(true);
                    imageReference.UpdateImages();
                }
                stateManager.LoadJSONGame(texture, pieces, data.rows, data.columns, data.generationSeed);
            }
        }
    }
}
