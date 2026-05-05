//using System;
//using System.IO;
//using System.Collections.Generic;
//using System.Text.Json;

//namespace JSONFunctions
//{
//    public class JSONFileFunctions
//    {
//        [Serializable]
//        public class PieceData
//        {
//            public int Id;
//            public int Row;
//            public int Column;

//            public EdgeType TopEdge;
//            public EdgeType RightEdge;
//            public EdgeType BottomEdge;
//            public EdgeType LeftEdge;

//            public float PositionX;
//            public float PositionY;
//            public float Rotation;

//        }

//        [Serializable]
//        public class PuzzleSaveData
//        {
//            public string imageFilePath;
//            public int rows;
//            public int columns;
//            public int generationSeed;
//            public List<PieceData> pieces;
//        }


//        public static void CreateOrEditJSON(PuzzleSaveData data, string filepath)
//        {
//            try
//            {
//                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
//                {
//                    WriteIndented = true
//                });

//                File.WriteAllText(filepath, json);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Error in CreateOrEditJSON: " + ex.Message);
//            }
//        }

//        public static PuzzleSaveData ReadJSON(string filepath)
//        {
//            try
//            {
//                string json = File.ReadAllText(filepath);
//                return JsonSerializer.Deserialize<PuzzleSaveData>(json);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Error in ReadJSON:" + ex.Message);
//                return null;
//            }
//        }
//    }
//}
