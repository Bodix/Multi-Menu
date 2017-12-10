using System;
using System.Collections.Generic;

namespace Library
{
    class Menu<T>
    {
        public Menu(List<T> items)
        {
            _items = items;
            KeyInstructions = (key) =>
            {
                if (key == ConsoleKey.UpArrow)
                {
                    SelectedItem--;
                }
                if (key == ConsoleKey.DownArrow)
                {
                    SelectedItem++;
                }
                if (key == ConsoleKey.Backspace)
                {
                    Console.Clear();
                    Console.CursorVisible = true;
                    IsClosed = true;
                }
            };
            FirstVisibleItem = 0;
            LastVisibleItem = (_items.Count > 10 ? 9 : _items.Count - 1);
            Help = "↑, ↓ - перемещение \n";
        }

        List<T> _items;
        int selectedItem;
        int firstVisibleItem;
        int lastVisibleItem;
        public delegate void KeyHandler(ConsoleKey key);
        public delegate string ItemHandler(T item);
        public delegate void AddItemHandler();

        public string Info { get; set; }
        public string Empty { get; set; }
        public string Help { get; set; }
        public int SelectedItem
        {
            get
            {
                if (selectedItem >= _items.Count)
                {
                    selectedItem = _items.Count - 1;
                }
                return selectedItem;
            }
            set
            {
                if (value < 0) value = 0;
                if (value > _items.Count - 1) value = _items.Count - 1;
                if (value < FirstVisibleItem)
                {
                    ScrollUp();
                }
                if (value > LastVisibleItem)
                {
                    ScrollDown();
                }
                selectedItem = value;
            }
        }
        private int FirstVisibleItem
        {
            get
            {
                return firstVisibleItem;
            }
            set
            {
                if (value < 0) value = 0;
                firstVisibleItem = value;
            }
        }
        private int LastVisibleItem
        {
            get
            {
                return lastVisibleItem;
            }
            set
            {
                if (value > _items.Count - 1) value = _items.Count - 1;
                lastVisibleItem = value;
            }
        }
        public Tuple<ConsoleColor, string>[] ColorInfo { get; set; }
        public KeyHandler KeyInstructions { get; set; }
        public ItemHandler ItemInstruction { get; set; }
        public AddItemHandler AddItemInstruction { get; set; }
        public bool IsClosed { get; set; }

        public void Use()
        {
            IsClosed = false;
            Console.CursorVisible = false;
            Console.Clear();

            while (!IsClosed)
            {
                Update();
                ReadKey();
            }
        }
        void Update()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(Info + "\n");
            if (_items.Count != 0)
            {
                Console.WriteLine((FirstVisibleItem == 0) ? " " : "↑");
                for (int i = FirstVisibleItem; i <= LastVisibleItem; i++)
                {
                    Console.Write(i != SelectedItem ? "  " : "o ");
                    Console.WriteLine(String.Format("{0,-77}", ItemInstruction(_items[i])));
                    Console.ResetColor();
                }
                Console.WriteLine((LastVisibleItem == _items.Count - 1) ? " " : "↓");
            }
            else Console.WriteLine("\n  " + Empty + "\n");
            if (ColorInfo != null)
            {
                foreach (Tuple<ConsoleColor, string> color in ColorInfo)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = color.Item1;
                    Console.Write("■");
                    Console.ResetColor();
                    Console.Write(" - " + color.Item2 + " ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("\n" + Help);
        }
        void ReadKey()
        {
            var key = Console.ReadKey().Key;
            KeyInstructions.Invoke(key);
        }
        public void AddItem()
        {
            Console.Clear();
            Console.CursorVisible = true;
            AddItemInstruction.Invoke();
            Console.CursorVisible = false;
            Console.Clear();
        }
        public void DeleteSelectedItem()
        {
            if (_items.Count != 0)
            {
                _items.Remove(_items[SelectedItem]);
                Console.Clear();
                if (LastVisibleItem >= _items.Count)
                {
                    LastVisibleItem = _items.Count - 1;
                    FirstVisibleItem--;
                }
            }
        }
        private void ScrollUp()
        {
            FirstVisibleItem--;
            LastVisibleItem--;
            if (lastVisibleItem - firstVisibleItem < 9)
                firstVisibleItem = lastVisibleItem - 9;
        }
        private void ScrollDown()
        {
            FirstVisibleItem++;
            LastVisibleItem++;
            if (lastVisibleItem - firstVisibleItem < 9)
                lastVisibleItem = firstVisibleItem + 9;
        }
    }
}
