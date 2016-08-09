using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Drawing.Printing;
using System.IO;

namespace ForAliceWpfApplication
{


    public class FeelWords
    {

       public void DoWork()
        {

            var Files = new List<string>();
            Files.AddRange(Directory.GetFiles(".", "*.mov"));
            Files.AddRange(Directory.GetFiles(".", "*.dct"));
            Files.AddRange(Directory.GetFiles(".", "*.txt"));

            int MinimumWordLength = 5; // for a feelword
            var RandomSeed = (new Random()).Next();
            var Rnd = new Random(RandomSeed); // we can determine seed and repeat some calculations

            var FWD = new FWDictionary(Rnd, MinimumWordLength); // load all words
            foreach (string filename in Files)
            {
                FWD.AddFile(Path.GetFileName(filename).ToString());
            }

            int FeelwordsCount = 1, FeelwordWidth = 12, FeelwordHeight = 12;
            int DeadCellLimit = 1;

            int TotalSamples = 0;


            var Feelwords = GenerateFeelwords(FeelwordWidth, FeelwordHeight, FeelwordsCount, FWD, MinimumWordLength, DeadCellLimit, out TotalSamples);
            for (int i = 0; i < Feelwords.Count; i++)
            {
                Feelwords[i].WordsShuffle();
                //var mm = new List<char>();

                for (int c = 0; c < Feelwords[i].Width; c++)
                {
                    for (int r = 0; r < Feelwords[i].Height; r++)
                    {
                        string mmm = Feelwords[i].Cells[c, r].Char.ToString();
                    }
                }
            }
        }

        public class ProcessState
        {
            public string StatePrefix { get; private set; }
            public int ProgressValue { get; private set; }
            public int ProgressMaximum { get; private set; }
            public string StateSuffix { get; private set; }

            public ProcessState(string StatePrefix, int ProgressValue, int ProgressMaximum, string StateSuffix)
            {
                this.StatePrefix = StatePrefix;
                this.ProgressValue = ProgressValue;
                this.ProgressMaximum = ProgressMaximum;
                this.StateSuffix = StateSuffix;
            }
        }

        /// <summary>Generate needed number of feelwords</summary>
        List<FWInfo> GenerateFeelwords(int Width, int Height, int Count, FWDictionary FWD, int MinimumWordLength, int DeadCellsLimit, out int TotalSamples)
        {
            List<FWInfo> Result = new List<FWInfo>();
            TotalSamples = 0;
            while (Result.Count < Count)
            {
                var fw = new FWInfo(FWD.Rnd, Width, Height, FWD);
                try
                {
                    fw.Generate(MinimumWordLength);
                    TotalSamples++;
                    if (fw.DeadCells.Count <= DeadCellsLimit)
                        Result.Add(fw);
                }
                catch (StopGeneratingException)
                {
                    // no actions, just failed generation
                }
            }

            return Result;
        }
    }

    public class FWDictionary : Dictionary<int, List<string>>
    {
        private int MinWordLength { get; set; }

        public Random Rnd { get; set; }

        public FWDictionary(Random Rnd, int MinWordLength)
        {
            this.MinWordLength = MinWordLength;
            this.Rnd = Rnd;
        }

        public void AddWord(string word)
        {
            if (!ContainsKey(word.Length))
                Add(word.Length, new List<string>());
            this[word.Length].Add(word);
        }

        public void AddFile(string FileName)
        {
            // загрузка словаря
            string Extension = Path.GetExtension(FileName);
            string[] FileLines = File.ReadAllLines(FileName, Encoding.GetEncoding(1251));
            foreach (string Line in FileLines)
            {
                string[] LineParts;
                string Word;
                switch (Extension)
                {
                    case ".mov":
                        if (Line.Contains("="))
                        {
                            LineParts = Line.Split('=');
                            Word = LineParts[0];
                            if (LineParts != null && LineParts.Length >= 1 && Word.Length >= 2 && Word.Length >= MinWordLength && !Word.Contains("-") && !Word.Trim().Contains(" "))
                                AddWord(Word.Trim().ToUpper().Replace("!", "").Replace(".", "").Replace(",", ""));
                        }
                        break;
                    case ".dct":
                        LineParts = Line.Split('\t');
                        if (LineParts.Length >= 2)
                        {
                            Word = LineParts[2];
                            if (Word.Length >= 2 && Word.Length >= MinWordLength)
                                AddWord(Word.Trim().ToUpper().Replace("!", "").Replace(".", "").Replace(",", ""));
                        }
                        break;
                    case ".txt":
                        AddWord(Line.Trim().ToUpper());
                        break;
                    default:
                        throw new Exception("Неизвестное расширение (формат) словаря");
                }
            }
        }

        public int WordsCount
        {
            get
            {
                int Result = 0;
                foreach (var kvp in this)
                    Result += kvp.Value.Count;
                return Result;
            }
        }


        public int GetRandomWordIndexExact(int Length) // вернуть слово указанной длины
        {
            if (!ContainsKey(Length))
                throw new NoExactLengthInDictionaryException(string.Format("There were no words in the dictionary with a length: {0}", Length), Length);
            return Rnd.Next(this[Length].Count);
        }

        public string GetRandomWordExact(List<string> UnqiueWords, int Length) // вернуть слово указанной длины, уникальное среди переданного списка
        {
            var FailedIndexes = new List<int>();
            while (true)
            {
                int CurrentIndex = -1;

                while (CurrentIndex < 0 || Length < MinimumLength)
                {
                    try
                    {
                        CurrentIndex = GetRandomWordIndexExact(Length);
                    }
                    catch (NoExactLengthInDictionaryException)
                    {
                        Length--;
                    }
                }

                if (!FailedIndexes.Contains(CurrentIndex))
                {
                    string CurrentWord = this[Length][CurrentIndex];
                    if (!UnqiueWords.Contains(CurrentWord))
                        return CurrentWord;
                    else
                        FailedIndexes.Add(CurrentIndex);
                }
                if (FailedIndexes.Count >= this[Length].Count) // if count of failed indexes >= total words of the needed length
                    throw new NoWordsInDictionaryException("No words of needed length", Length);
            }
        }

        public string GetRandomWordNotLess(int MinLength = 0) // вернуть слово любой длины не меньше указанной
        {
            // random length
            var Lengths = this.Keys.ToList();
            for (int i = 0; i < Lengths.Count; i++)
                if (Lengths[i] < MinLength)
                {
                    Lengths.RemoveAt(i);
                    i--;
                }

            if (Lengths.Count == 0)
                throw new Exception(string.Format("No words founded with a minimum length: {0}", MinLength));

            int Length = Lengths[Rnd.Next(Lengths.Count)];
            return this[Length][Rnd.Next(Rnd.Next(this[Length].Count))];
        }

        public string GetRandomWordNotLess(List<string> UnqiueWords, int MinLength = 0) // вернуть слово любой длины не меньше указанной, уникальное среди переданного списка
        {
            while (true)
            {
                string CurrentWord = GetRandomWordNotLess(MinLength);
                if (!UnqiueWords.Contains(CurrentWord))
                    return CurrentWord;
            }
        }

        public int MaximumLength { get { return this.Keys.Max(); } }
        public int MinimumLength { get { return Math.Max(this.Keys.Min(), MinWordLength); } }

    }

    public enum FWDirections
    {
        Unknown = 0,
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4,
        Last = 5
    }

    public class NoSpaceForLengthException : Exception
    {
        public string Word { get; private set; }
        public int MaxLengthAvailable { get; private set; }

        public NoSpaceForLengthException(string Message, string Word, int MaxLengthAvailable)
            : base(Message)
        {
            this.Word = Word;
            this.MaxLengthAvailable = MaxLengthAvailable;
        }
    }

    public class StopGeneratingException : Exception
    {
        public StopGeneratingException(string Message)
            : base(Message)
        { }
    }

    public class NoWordsInDictionaryException : Exception
    {
        public int Length { get; private set; }

        public NoWordsInDictionaryException(string Message, int Length)
            : base(Message)
        {
            this.Length = Length;
        }
    }

    public class NoExactLengthInDictionaryException : Exception
    {
        public int Length { get; private set; }

        public NoExactLengthInDictionaryException(string Message, int Length)
            : base(Message)
        {
            this.Length = Length;
        }
    }

    public class FWWordInfo : List<FWWordCharInfo>
    {
        public string Word { get; private set; }
        public FWInfo Feelword { get; private set; }

        public Random Rnd { get { return Feelword.Rnd; } }

        // разместить слово Word в матрице филворда, начиная с ячейки Cell
        public FWWordInfo(string Word, FWInfo FW)
        {
            this.Word = Word;
            this.Feelword = FW;

            Clear();
            //FW.TempUsedCells.Clear();

            var Contours = FW.GetAvailableContours(); // gets all available contours
            var ContoursCandidatesExactLength = new List<int>(); // contours with enough space to word placement (with a same length as a word)
            var ContoursCandidatesMoreLength = new List<int>(); // contours with enough space to word placement (with more space than needed)
            int MaxContourLength = 0;
            // Analyze Contours
            foreach (var kvp in Contours)
            {
                // if contour can take the word - add it into candidates list
                if (kvp.Value.Count == Word.Length) ContoursCandidatesExactLength.Add(kvp.Key);
                if (kvp.Value.Count > Word.Length) ContoursCandidatesMoreLength.Add(kvp.Key);

                if (kvp.Value.Count > MaxContourLength) // получаем максимальную длину контура
                    MaxContourLength = kvp.Value.Count;
            }

            // select random contour for word placement
            int SelectedContourID;
            if (ContoursCandidatesExactLength.Count > 0)
                SelectedContourID = ContoursCandidatesExactLength[Rnd.Next(ContoursCandidatesExactLength.Count)];
            else if (ContoursCandidatesMoreLength.Count > 0)
                SelectedContourID = ContoursCandidatesMoreLength[Rnd.Next(ContoursCandidatesMoreLength.Count)];
            else
                throw new NoSpaceForLengthException(string.Format("Cannot select contour for word's length {0}. Maximum length is {1}", Word.Length, MaxContourLength), Word, MaxContourLength);

            // (!) if we are here, we 90% can place word in the contour (it depend by path configuration)

            var CurrentContour = Contours[SelectedContourID]; // gets contour which we'll use to place the word

            int PossibilitesLimit = 4;
            int MinPossibilites = int.MaxValue;
            var StartCellCandidates = new Dictionary<int, List<FWCellInfo>>(); // int - for possibilities
            if (Contours.Count == 1 && CurrentContour.Count == FW.TotalCellsCount) // if we have only one contour and its collect all cells => we just start with our first word and can select any cell
            {
                MinPossibilites = 1;
                StartCellCandidates.Add(MinPossibilites, (from x in CurrentContour select x).ToList());
            }
            else
            {
                // check all cells in the contour and select a cell with minimum possible numbers of variants (corner or dead-end cells)
                foreach (var cell in CurrentContour)
                {
                    var Possibilities = Feelword.GetPossibleFreeCellDirectionsCount(cell, true, false);
                    if (Possibilities < MinPossibilites)
                        MinPossibilites = Possibilities;
                    if (!StartCellCandidates.ContainsKey(Possibilities))
                        StartCellCandidates.Add(Possibilities, new List<FWCellInfo>());
                    StartCellCandidates[Possibilities].Add(cell);
                }
            }

            var StartCellsList = StartCellCandidates[MinPossibilites];

          //var StartDeadCellsCount = FW.GetDeadCellsCount(true);

        lblSelectAnotherStartCell:
            while (StartCellsList == null || StartCellsList.Count == 0)
            {
                MinPossibilites++;
                if (MinPossibilites > PossibilitesLimit)
                {
                    if (Word.Length > Feelword.Dictionary.MinimumLength)
                        throw new NoSpaceForLengthException(string.Format("No possibilities to place the word's length {0}. Lets try length {1}", Word.Length, Word.Length - 1), Word, Word.Length - 1);
                    else
                        throw new StopGeneratingException("No cells with needed possibilities"); // we have to stop generating feelword after this exception was catched
                }

                //
                StartCellCandidates.TryGetValue(MinPossibilites, out StartCellsList); // can get exception here is 
            }
            FW.TempUsedCells.Clear();

            var Index = Rnd.Next(StartCellsList.Count);  // receive a random cell index in the contour
            var StartCell = StartCellsList[Index]; // Current cell in the path of the word
            var Cell = StartCell;
            //var ForbiddenVectors = new List<KeyValuePair<FWCellInfo, FWCellInfo>>(); // List of forbidden directions from one cell to another

            var ForbiddenPaths = new List<KeyValuePair<string, List<FWCellInfo>>>(); // List of forbidden paths (Path is a list of cells). Key - is tempused cells configuration for which FP is really forbidden
            // the next goal is decide where to go next from the cell
            for (int i = 0; i < Word.Length; i++)
            {
                Cell.MarkAsTempUsed(); // mark current cell as temporary used

                var C = new FWWordCharInfo(this); // create a new symbol
                C.Symbol = Word[i];
                if (i != Word.Length - 1)
                {
                    var AdjacentCells = FW.GetAdjacentCells(Cell, CurrentContour, ForbiddenPaths, true); // возвращает список всех ячеек, смежных с текущей, в которой можно разместить следующую букву слова
                    if (AdjacentCells.Count == 0)
                    {
                        if (i == 0) // if we just begin
                        {
                            StartCellsList.Remove(StartCell); // remove unhappy start point
                            goto lblSelectAnotherStartCell;
                        }
                        else
                        {
                            // overwise go back and try to find out a fork
                            var ForbiddenPath = new List<FWCellInfo>();
                            int j;
                            for (j = i - 1; j >= 0; j--)
                            {
                                var TempAdjCells = FW.GetAdjacentCells(Cell, CurrentContour, ForbiddenPaths, true);

                                ForbiddenPath.Add(Cell);
                                //Cell.UnmarkAsTempUsed();

                                if (TempAdjCells.Count > 0)
                                {
                                    foreach (var cell in ForbiddenPath) cell.UnmarkAsTempUsed();
                                    ForbiddenPath.Reverse();
                                    // save tempused ID for which forbiddenpath is actual
                                    var FPID = string.Format("{0}{1};", FW.TempUsedCellsID, ForbiddenPath[0].ID); // and add first cell of FP
                                    ForbiddenPaths.Add(new KeyValuePair<string, List<FWCellInfo>>(FPID, ForbiddenPath));
                                    break;
                                }
                                else
                                {
                                    Cell = this[j].Cell;
                                    this.RemoveAt(j);

                                    if (j == 0)
                                    {
                                        StartCellsList.Remove(StartCell); // remove unhappy start point
                                        goto lblSelectAnotherStartCell;
                                    }
                                }
                            }
                            i = j;
                            continue;
                            /*// overwise, add a prohibition
                            ForbiddenVectors.Add(new KeyValuePair<FWCellInfo, FWCellInfo>(this[i - 1].Cell, Cell)); // forbid path from previous cell to current
                            Cell.UnmarkAsTempUsed(); // cell already unmarked
                            Cell = this[i - 1].Cell; // move back to previous cell
                            this.RemoveAt(i - 1); // remove last added symbol
                            i -= 2; // go step back
                            continue;*/
                        }
                        //throw new Exception("No adjacent cells for the cell in the contour");
                    }
                    var NextCell = AdjacentCells[Rnd.Next(AdjacentCells.Count)]; // select random cell
                    C.NextDirection = Feelword.GetDirection(Cell, NextCell);
                    C.Cell = Cell;
                    Cell = NextCell;
                }
                else
                    C.NextDirection = FWDirections.Last;
                this.Add(C);
                //ForbiddenVectors.Clear(); // clear forbidden paths if we can go farther
            }

            // add word into feelword
            Cell = StartCell;
            foreach (var c in this)
            {
                FW.Cells[Cell.X, Cell.Y].Char = c;
                if (c.NextDirection != FWDirections.Last)
                {
                    var NextCell = Feelword.GetNextCellByDirection(Cell, c.NextDirection);
                    if (NextCell == null)
                        throw new Exception("Next cell is null");
                    Cell = NextCell;
                }
            }
            Feelword.TempUsedCells.Clear();
        }

        public string Export()
        {
            StringBuilder Result = new StringBuilder();
            foreach (var c in this)
            {
                Result.AppendFormat("'{0}' {1}; ", c.Symbol, c.NextDirection);
            }
            return Result.ToString();
        }
    }

    public class FWWordCharInfo
    {

        public FWWordInfo Word { get; private set; } // какому слову принадлежит

        public char Symbol { get; set; } // символ
        public FWCellInfo Cell { get; set; } // ячейка, в которой располагается символ

        public FWDirections NextDirection { get; set; } // направление следующего символа    

        public FWWordCharInfo(FWWordInfo Word)
        {
            this.Word = Word;
            this.Cell = null;
            this.NextDirection = FWDirections.Unknown;
        }
    }

    public enum FWCellStates
    {
        Empty = 0,
        Char = 1,
        Dead = 2
    }

    public class FWCellInfo
    {
        public FWInfo Feelword { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public string ID { get { return string.Format("{0}-{1}", X, Y); } }

        public FWWordCharInfo Char { get; set; }

        public FWCellStates State { get { return IsDead ? FWCellStates.Dead : (Char != null ? FWCellStates.Char : FWCellStates.Empty); } }

        public FWCellInfo(FWInfo Feelword, int X, int Y)
        {
            this.Feelword = Feelword;
            this.X = X;
            this.Y = Y;
            this.Char = null;
        }

        public bool IsEmpty { get { return State == FWCellStates.Empty; } }
        public bool IsTempUsed { get { return Feelword.TempUsedCells.ContainsKey(this); } }
        public bool IsDead { get { return Feelword.DeadCells.ContainsKey(this); } }

        public void MarkAsTempUsed()
        {
            if (!IsTempUsed)
                Feelword.TempUsedCells.Add(this, 0);
        }

        public void UnmarkAsTempUsed()
        {
            if (IsTempUsed)
                Feelword.TempUsedCells.Remove(this);
        }

        public void MarkAsDead()
        {
            if (!IsDead)
                Feelword.DeadCells.Add(this, 0);
        }

        public void UnmarkAsDead()
        {
            if (IsDead)
                Feelword.DeadCells.Remove(this);
        }

        /// <summary>Возвращает true, если текущая ячейка расположена рядом, на пути</summary>
        public bool IsAdjacent(FWCellInfo Cell)
        {
            bool OnOneRow = this.X == Cell.X, OnOneColumn = this.Y == Cell.Y;
            bool AdjacentByRow = Math.Abs(this.X - Cell.X) <= 1, AdjacentByColumn = Math.Abs(this.Y - Cell.Y) <= 1;
            return (Cell != this) && ((OnOneRow && AdjacentByColumn) || (OnOneColumn && AdjacentByRow));
        }
    }

    public class FWInfo
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Random Rnd { get; private set; }

        public FWCellInfo[,] Cells { get; private set; }
        public Dictionary<FWCellInfo, int> TempUsedCells { get; private set; }
        public Dictionary<FWCellInfo, int> DeadCells { get; private set; }
        public FWDictionary Dictionary { get; private set; }

        /// <summary>Available after generation</summary>
        public List<string> Words { get; private set; }

        public string TempUsedCellsID
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var kvp in TempUsedCells)
                {
                    sb.Append(kvp.Key.ID);
                    sb.Append(';');
                }
                return sb.ToString();
            }
        }

        public FWInfo(Random Rnd, int Width, int Height, FWDictionary WordsDictionary)
        {
            this.Width = Width;
            this.Height = Height;
            this.Dictionary = WordsDictionary;
            this.Rnd = Rnd;

            Cells = new FWCellInfo[Width, Height];
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Cells[i, j] = new FWCellInfo(this, i, j);

            TempUsedCells = new Dictionary<FWCellInfo, int>();
            DeadCells = new Dictionary<FWCellInfo, int>();
        }

        /*
        public List<FWWordInfo> Words
        {
          get
          {
            var Result = new Dictionary<FWWordInfo, byte>();
            for (int i = 0; i < Width; i++)
              for (int j = 0; j < Height; j++)
                if (Cells[i, j].Char != null)
                {
                  var Word = Cells[i, j].Char.Word;
                  if (!Result.ContainsKey(Word))
                    Result.Add(Word, 0);
                }
            return Result.Keys.ToList();
          }
        } */

        public int TotalCellsCount { get { return Width * Height; } }

        public int FreeCellsCount
        {
            get
            {
                int Result = 0;
                for (int i = 0; i < Width; i++)
                    for (int j = 0; j < Height; j++)
                        if (Cells[i, j].IsEmpty)
                            Result++;
                return Result;
            }
        }

        /// <summary>Возвращает направление от From к To. Заодно проверяет то, что они рядом</summary>
        public FWDirections GetDirection(FWCellInfo FromCell, FWCellInfo ToCell)
        {
            if (!FromCell.IsAdjacent(ToCell))
                throw new Exception("Cells not adjacent");
            if (ToCell.X == FromCell.X && ToCell.Y == FromCell.Y - 1) return FWDirections.Up;
            else if (ToCell.X == FromCell.X && ToCell.Y == FromCell.Y + 1) return FWDirections.Down;
            else if (ToCell.X == FromCell.X - 1 && ToCell.Y == FromCell.Y) return FWDirections.Left;
            else if (ToCell.X == FromCell.X + 1 && ToCell.Y == FromCell.Y) return FWDirections.Right;
            else throw new Exception("Strange direction");
        }

        public FWCellInfo GetNextCellByDirection(FWCellInfo Current, FWDirections Direction)
        {
            switch (Direction)
            {
                case FWDirections.Up: return Current.Y > 0 ? Cells[Current.X, Current.Y - 1] : null;
                case FWDirections.Down: return Current.Y < Height - 1 ? Cells[Current.X, Current.Y + 1] : null;
                case FWDirections.Left: return Current.X > 0 ? Cells[Current.X - 1, Current.Y] : null;
                case FWDirections.Right: return Current.X < Width - 1 ? Cells[Current.X + 1, Current.Y] : null;
                default:
                    throw new Exception("Invalid direction: " + Direction);
            }
        }


        /// <summary>Возвращает ячейки вокруг переданной. Если null, то ячейка не существует</summary>
        public void GetCellsAround(FWCellInfo Cell,
          out FWCellInfo Left, out FWCellInfo Right, out FWCellInfo Up, out FWCellInfo Down,
          out FWCellInfo LeftUp, out FWCellInfo LeftDown, out FWCellInfo RightUp, out FWCellInfo RightDown)
        {
            Left = Cell.X > 0 ? Cells[Cell.X - 1, Cell.Y] : null;
            Right = Cell.X < Width - 1 ? Cells[Cell.X + 1, Cell.Y] : null;

            Up = Cell.Y > 0 ? Cells[Cell.X, Cell.Y - 1] : null;
            Down = Cell.Y < Height - 1 ? Cells[Cell.X, Cell.Y + 1] : null;

            LeftUp = Cell.X > 0 && Cell.Y > 0 ? Cells[Cell.X - 1, Cell.Y - 1] : null;
            LeftDown = Cell.X > 0 && Cell.Y < Height - 1 ? Cells[Cell.X - 1, Cell.Y + 1] : null;

            RightUp = Cell.X < Width - 1 && Cell.Y > 0 ? Cells[Cell.X + 1, Cell.Y - 1] : null;
            RightDown = Cell.X < Width - 1 && Cell.Y < Height - 1 ? Cells[Cell.X + 1, Cell.Y + 1] : null;
        }

        public FWCellInfo GetNextFreeCellByDirection(FWCellInfo Current, FWDirections Direction, bool AllowEmpty, bool AllowTempUsed)
        {
            FWCellInfo NextCell = GetNextCellByDirection(Current, Direction);
            if (NextCell != null && !NextCell.IsEmpty) NextCell = null;
            if (NextCell != null && !AllowEmpty && NextCell.IsEmpty) NextCell = null;
            if (NextCell != null && !AllowTempUsed && NextCell.IsTempUsed) NextCell = null;
            return NextCell;
        }

        private void GetPossibleFreeCellDirections(FWCellInfo Cell, bool AllowEmpty, bool AllowTempUsed, out bool CanLeft, out bool CanRight, out bool CanUp, out bool CanDown)
        {
            FWCellInfo LeftCell = GetNextFreeCellByDirection(Cell, FWDirections.Left, AllowEmpty, AllowTempUsed);
            CanLeft = LeftCell != null ? LeftCell.State == FWCellStates.Empty : false;

            FWCellInfo RightCell = GetNextFreeCellByDirection(Cell, FWDirections.Right, AllowEmpty, AllowTempUsed);
            CanRight = RightCell != null ? RightCell.State == FWCellStates.Empty : false;

            FWCellInfo UpCell = GetNextFreeCellByDirection(Cell, FWDirections.Up, AllowEmpty, AllowTempUsed);
            CanUp = UpCell != null ? UpCell.State == FWCellStates.Empty : false;

            FWCellInfo DownCell = GetNextFreeCellByDirection(Cell, FWDirections.Down, AllowEmpty, AllowTempUsed);
            CanDown = DownCell != null ? DownCell.State == FWCellStates.Empty : false;
        }

        /// <summary>Возвращает число возможных степеней свободы для ячейки</summary>
        public int GetPossibleFreeCellDirectionsCount(FWCellInfo Cell, bool AllowEmpty, bool AllowTempUsed)
        {
            bool CanLeft, CanRight, CanUp, CanDown;
            GetPossibleFreeCellDirections(Cell, AllowEmpty, AllowTempUsed, out CanLeft, out CanRight, out CanUp, out CanDown);
            return (CanLeft ? 1 : 0) + (CanRight ? 1 : 0) + (CanUp ? 1 : 0) + (CanDown ? 1 : 0);
        }

        /*public void GetPossibleFreeCellDirections(FWCellInfo Cell, bool AllowEmpty, bool AllowTempUsed, out List<FWDirections> PossibleDirections)
        {
          bool CanLeft, CanRight, CanUp, CanDown;

          PossibleDirections = new List<FWDirections>();
          GetPossibleFreeCellDirections(Cell, AllowEmpty, AllowTempUsed, out CanLeft, out CanRight, out CanUp, out CanDown);
          if (CanLeft) PossibleDirections.Add(FWDirections.Left);
          if (CanRight) PossibleDirections.Add(FWDirections.Right);
          if (CanUp) PossibleDirections.Add(FWDirections.Up);
          if (CanDown) PossibleDirections.Add(FWDirections.Down);
        }

        public void GetPossibleFreeCellDirections(FWCellInfo Cell, bool AllowEmpty, bool AllowTempUsed, out List<FWCellInfo> PossibleCells)
        {
          bool CanLeft, CanRight, CanUp, CanDown;

          PossibleCells = new List<FWCellInfo>();
          GetPossibleFreeCellDirections(Cell, AllowEmpty, AllowTempUsed, out CanLeft, out CanRight, out CanUp, out CanDown);
          if (CanLeft) PossibleCells.Add(GetNextCellByDirection(Cell, FWDirections.Left));
          if (CanRight) PossibleCells.Add(GetNextCellByDirection(Cell, FWDirections.Right));
          if (CanUp) PossibleCells.Add(GetNextCellByDirection(Cell, FWDirections.Up));
          if (CanDown) PossibleCells.Add(GetNextCellByDirection(Cell, FWDirections.Down));
        }*/

        private void AddCellToKeysDictionary(FWCellInfo Cell, Dictionary<FWCellInfo, int> Cells)
        {
            if (!Cells.ContainsKey(Cell)) Cells.Add(Cell, 0);
        }

        /// <summary>Возвращает все возможные несвязанные контуры, вокруг заполненных данных. int - идентификатор контура. В контуре ТОЛЬКО ПУСТЫЕ ячейки!</summary>
        public Dictionary<int, List<FWCellInfo>> GetAvailableContours(bool TempUsedAreNotEmpty = false)
        {
            var NextToCells = new Dictionary<FWCellInfo, int>(); // список всех пустых ячеек, расположенных рядом с занятыми
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    /* // no contour
                     if (Cells[i, j].IsEmpty) // if cell is empty - add to candidate
                      AddCellToKeysDictionary(Cells[i, j], NextToCells);*/

                    // real contour
                    if (!Cells[i, j].IsEmpty || (TempUsedAreNotEmpty && Cells[i, j].IsTempUsed)) // если ячейка заполнена (или временно используется), то ищем пустые ячейки вокруг неё и добавляем в список кандидатов
                    {
                        FWCellInfo LeftCell, RightCell, UpCell, DownCell, LeftUpCell, LeftDownCell, RightUpCell, RightDownCell;

                        GetCellsAround(Cells[i, j], out LeftCell, out RightCell, out UpCell, out DownCell, out LeftUpCell, out LeftDownCell, out RightUpCell, out RightDownCell);

                        // основные ячейки
                        if (LeftCell != null && LeftCell.IsEmpty) AddCellToKeysDictionary(LeftCell, NextToCells);
                        if (RightCell != null && RightCell.IsEmpty) AddCellToKeysDictionary(RightCell, NextToCells);
                        if (UpCell != null && UpCell.IsEmpty) AddCellToKeysDictionary(UpCell, NextToCells);
                        if (DownCell != null && DownCell.IsEmpty) AddCellToKeysDictionary(DownCell, NextToCells);

                        if (LeftUpCell != null && LeftUpCell.IsEmpty) AddCellToKeysDictionary(LeftUpCell, NextToCells);
                        if (LeftDownCell != null && LeftDownCell.IsEmpty) AddCellToKeysDictionary(LeftDownCell, NextToCells);
                        if (RightUpCell != null && RightUpCell.IsEmpty) AddCellToKeysDictionary(RightUpCell, NextToCells);
                        if (RightDownCell != null && RightDownCell.IsEmpty) AddCellToKeysDictionary(RightDownCell, NextToCells);
                    }
                }

            // вдруг нет заполненных ячеек
            if (NextToCells.Count == 0) // если нет занятых ячеек, то добавляем все ячейки в список контура
            {
                for (int i = 0; i < Width; i++)
                    for (int j = 0; j < Height; j++)
                        if (Cells[i, j].IsEmpty)
                            AddCellToKeysDictionary(Cells[i, j], NextToCells);
            }

            var Contours = new Dictionary<int, List<FWCellInfo>>();

            // разбиваем ячейки на контуры
            int CurrentContourID = 0;
            var CellsList = NextToCells.Keys.ToList();
            while (CellsList.Count > 0)
            {
                CurrentContourID++;
                Contours.Add(CurrentContourID, new List<FWCellInfo>());
                AddIntoContour(CellsList[0], Contours[CurrentContourID], CellsList);
            }

            return Contours;
        }

        private void AddIntoContour(FWCellInfo Cell, List<FWCellInfo> Contour, List<FWCellInfo> CandidatesList)
        {
            Contour.Add(Cell);
            CandidatesList.Remove(Cell);

            for (int i = 0; i < CandidatesList.Count; i++) // добавляем всех соседей в тот же контур
                if (Cell.IsAdjacent(CandidatesList[i]))
                {
                    AddIntoContour(CandidatesList[i], Contour, CandidatesList);
                    i = 0; // and again
                }

            for (int i = 0; i < Contour.Count; i++) // for all cell in the contour we check all rest "non-contour" cells
                for (int j = 0; j < CandidatesList.Count; j++) // добавляем всех соседей в тот же контур
                    if (CandidatesList[j].IsAdjacent(Contour[i]))
                    {
                        AddIntoContour(CandidatesList[j], Contour, CandidatesList);
                        i = 0; // and again
                        j = 0;
                    }
        }

        /// <summary>Возвращает пустые ячейки, смежные с переданной, внутри переданного контура. 
        /// ForbiddenVectors: список запрешенных направлений из одной ячейки в другую
        /// Если ForbidTempUsed - то не добавляем в результат временно используемые ячейки</summary>
        public List<FWCellInfo> GetAdjacentCells(FWCellInfo Cell, List<FWCellInfo> Contour, List<KeyValuePair<string, List<FWCellInfo>>> ForbiddenPaths, bool ForbidTempUsed)
        {
            var TUCID = this.TempUsedCellsID;
            var ActualForbiddenPaths = (from p in ForbiddenPaths where p.Key == TUCID select p.Value).ToList();

            var Result = new List<FWCellInfo>();
            foreach (var item in Contour)
                if (Cell.IsAdjacent(item)) // если текущая ячейка примыкает к ячейке контура
                {
                    if (ForbidTempUsed && item.IsTempUsed)
                        continue;

                    bool IsPathProhibiled = false;
                    foreach (var Path in ActualForbiddenPaths)
                        if (Path.Count >= 2 && Path[0] == Cell && Path[1] == item)
                        {
                            IsPathProhibiled = true;
                            break;
                        }
                    if (IsPathProhibiled)
                        continue;

                    Result.Add(item);
                }
            return Result;
        }


        public int EstimateANumberOfDeadCells(bool TempUsedAreNotEmpty)
        {
            int Result = 0;
            var Contours = GetAvailableContours(TempUsedAreNotEmpty).Values.ToList();
            foreach (var Contour in Contours)
            {
                if (Contour.Count < Dictionary.MinimumLength)
                {
                    foreach (var cell in Contour)
                        Result++;
                }
            }
            return Result;
        }

        public void MarkCellsAsDead()
        {
            var Contours = GetAvailableContours().Values.ToList();
            foreach (var Contour in Contours)
            {
                if (Contour.Count < Dictionary.MinimumLength)
                {
                    foreach (var cell in Contour)
                        cell.MarkAsDead();
                }
            }
        }

        public new string ToString()
        {
            var sb = new StringBuilder();
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    var Cell = Cells[i, j];
                    sb.Append(Cell.IsDead ? "x" : (Cell.IsEmpty ? "_" : Cell.Char.Symbol.ToString()));
                    //sb.AppendFormat("{0}{1}", i, j);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public string TempUsedToString()
        {
            var sb = new StringBuilder();
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                    sb.Append(Cells[i, j].IsTempUsed ? "X" : "_");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void Generate(int MinWordLength)
        {
            Words = new List<string>();

            string[] WordList = { "УВЕРЕННОСТЬ", "САМОСТОЯТЕЛЬНОСТЬ", "ПРАВО", "РАДОСТЬ", "СВОБОДА", "ЖИЗНЬ" };
            int CurrentCounter = 0;
            while (FreeCellsCount - DeadCells.Count > 0)
            {
                string CurrentWord = "";
                if (CurrentCounter <= WordList.Count() - 1)
                {
                    CurrentWord = WordList[CurrentCounter];
                }
                CurrentCounter = CurrentCounter + 1;

                CurrentWord = SetCurrentWord(CurrentWord, MinWordLength);

                MarkCellsAsDead();

                // if all it ok - add the word to the List
                if (CurrentWord.Length != 0)
                    Words.Add(CurrentWord);
            }
        }

        public string SetCurrentWord(string CurrentWord, int MinWordLength)
        {

            if (CurrentWord == "")
            {
                CurrentWord = Dictionary.GetRandomWordNotLess(Words, MinWordLength); // generate a word
            }

            // place word into feelword
            while (true)
            {
                FWWordInfo fww;
                try
                {
                    fww = new FWWordInfo(CurrentWord, this);
                    //MessageBox.Show(ToString() + " " +Words.Count);
                    break;
                }
                catch (NoSpaceForLengthException e)
                {
                    if (e.MaxLengthAvailable >= Dictionary.MinimumLength)
                    {
                        try
                        {
                            CurrentWord = Dictionary.GetRandomWordExact(Words, e.MaxLengthAvailable);
                        }
                        catch (NoWordsInDictionaryException)
                        {
                            CurrentWord = "";
                            break;
                        }
                    }
                    else
                    {
                        CurrentWord = "";
                        break;
                    }
                }
                catch (StopGeneratingException)
                {
                    throw;
                }
            }

            return CurrentWord;
        }


        public void WordsShuffle()
        {
            if (Words == null)
                throw new Exception("Please, call Generate() first");
            int Iterations = 3;
            for (int k = 0; k < Iterations; k++)
                for (int i = 0; i < Words.Count; i++)
                {
                    int index = Rnd.Next(Words.Count);
                    string s = Words[i];
                    Words[i] = Words[index];
                    Words[index] = s;
                }
        }


        public string WordsToString()
        {
            string Result = "";
            // список слов
            foreach (string s in Words)
            {
                string s1 = s.Substring(0, 1).ToUpper();
                string s2 = s.Substring(1).ToLower();
                Result += s1 + s2 + ", ";
            }
            return Result.Substring(0, Result.Length - 2);
        }




    }


}
