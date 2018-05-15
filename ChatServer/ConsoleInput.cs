//namespace ChatServer
//{
//    using System;

//    /// <summary>
//    /// Class for getting console inputs. Not relivent to SyncIO Example
//    /// </summary>
//    class ConsoleExtentions
//    {
//        public static int GetDesision(string[] desision)
//        {
//            if (desision.Length < 1)
//                return 0;

//            for (int i = 0; i < desision.Length; i++)
//            {
//                Console.WriteLine("{0}] {1}", i + 1, desision[i]);
//            }

//            int des = -1;
//            while (des < 1 || !((des - 1) < desision.Length))
//            {
//                Console.Write(": ");
//                if (!int.TryParse(Console.ReadLine(), out des))
//                    des = -1;
//            }
//            return des - 1;
//        }

//        public static string GetNonEmptyString(string prompt)
//        {
//            var rt = string.Empty;
//            while (string.IsNullOrEmpty(rt))
//            {
//                Console.Write(prompt);
//                rt = Console.ReadLine();
//            }
//            return rt;
//        }

//        public static void ErrorAndClose(string err, params object[] ar)
//        {
//            Console.WriteLine(err, ar);
//            Console.ReadLine();
//            Environment.Exit(0);
//        }
//    }
//}