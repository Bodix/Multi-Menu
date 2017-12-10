using System;
using System.Collections.Generic;

namespace Library
{
    class Program
    {
        static List<User> users;
        static User currentUser;

        static void Main(string[] args)
        {
            Console.SetBufferSize(Console.WindowWidth, Console.WindowHeight);
            users = new List<User>
            {
                new Admin("admin", "123", "Администратор"),
                new User("user", "123", "Пользователь"),
            };

            while (true)
            {
                Authentication.IsLogged = false;
                while (currentUser == null)
                {
                    currentUser = Authentication.Login();
                }
                Authentication.IsLogged = true;

                List<string> menuItems = new List<string>();
                if (currentUser.GetType() == typeof(User))
                {
                    menuItems.Add("Книги");
                    menuItems.Add("Мои книги");
                }
                else if (currentUser.GetType() == typeof(Admin))
                {
                    menuItems.Add("Книги");
                    menuItems.Add("Пользователи");
                }
                menuItems.Add("Выйти из аккаунта");
                menuItems.Add("Закрыть");

                Menu<string> menu = new Menu<string>(menuItems)
                {
                    Info = "Здравствуйте, вы вошли как: " + currentUser.Name,
                    ItemInstruction = (item) =>
                    {
                        return item;
                    },
                };
                menu.KeyInstructions = (key) =>
                {
                    if (key == ConsoleKey.UpArrow)
                    {
                        menu.SelectedItem--;
                    }
                    if (key == ConsoleKey.DownArrow)
                    {
                        menu.SelectedItem++;
                    }
                };
                if (currentUser.GetType() == typeof(User))
                    menu.KeyInstructions += (key) =>
                    {
                        if (key == ConsoleKey.Enter)
                        {
                            if (menu.SelectedItem == 0)
                                Library.ShowBooks();
                            if (menu.SelectedItem == 1)
                                User.ShowBooks();
                            if (menu.SelectedItem == 2)
                            {
                                menu.IsClosed = true;
                                Authentication.Logout();
                            }
                            if (menu.SelectedItem == 3)
                                Environment.Exit(0);
                        }
                    };
                else if (currentUser.GetType() == typeof(Admin))
                    menu.KeyInstructions += (key) =>
                    {
                        if (key == ConsoleKey.Enter)
                        {
                            if (menu.SelectedItem == 0)
                                Library.ShowBooks();
                            if (menu.SelectedItem == 1)
                                Admin.ShowUsers();
                            if (menu.SelectedItem == 2)
                            {
                                menu.IsClosed = true;
                                Authentication.Logout();
                            }
                            if (menu.SelectedItem == 3)
                                Environment.Exit(0);
                        }
                    };
                menu.Help += "Enter - выбор";
                while (Authentication.IsLogged)
                {
                    menu.Use();
                }
            }
        }

        class Authentication
        {
            public static bool IsLogged;
            public static User Login()
            {
                User user = null;
                int userID = -1;
                bool loginCorrect;
                bool passwordCorrect;

                Console.Clear();
                do
                {
                    do
                    {
                        loginCorrect = false;
                        Console.WriteLine("Введите логин:");
                        string login = Console.ReadLine();
                        for (int i = 0; i < users.Count; i++)
                        {
                            if (login.ToLower() == users[i].Login)
                            {
                                userID = i;
                                loginCorrect = true;
                                break;
                            }
                        }
                        if (!loginCorrect)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Неверный логин \n");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                    }
                    while (!loginCorrect);
                    passwordCorrect = false;
                    Console.WriteLine("Введите пароль:");
                    string password = Console.ReadLine();
                    if (userID != -1 && password == users[userID].Password)
                    {
                        user = users[userID];
                        passwordCorrect = true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Неверный пароль \n");
                        Console.ForegroundColor = ConsoleColor.Gray;
                    }
                }
                while (!passwordCorrect);
                return user;
            }
            public static void Logout()
            {
                currentUser = null;
                IsLogged = false;
            }
        }
        class User
        {
            public User(string login, string password, string name)
            {
                Login = login;
                Password = password;
                Name = name;
            }

            static List<Book> takedBooks = new List<Book>();

            public string Login { get; set; }
            public string Password { get; set; }
            public string Name { get; set; }

            public static void ShowBooks()
            {
                Menu<Book> takedBooksMenu = new Menu<Book>(takedBooks)
                {
                    Info = "Мои книги:",
                    ItemInstruction = (item) =>
                    {
                        return item.Author + ", \"" + item.Name + "\" (" + item.Year + ")";
                    },
                    Empty = "Вы не брали книг"
                };
                takedBooksMenu.KeyInstructions += (key) =>
                {
                    if (key == ConsoleKey.Enter)
                    {
                        takedBooksMenu.DeleteSelectedItem();
                        if (takedBooks.Count != 0)
                            Library.MakeBookAvailable(takedBooks[takedBooksMenu.SelectedItem]);
                    }
                };
                takedBooksMenu.Help += String.Join("\n",
                    "Enter - вернуть книгу",
                    "Backspace - назад");
                takedBooksMenu.Use();
            }
            public void TakeBook(Book book)
            {
                if (book.IsAvailable)
                    takedBooks.Add(book);
            }
        }
        class Admin : User
        {
            public Admin(string login, string password, string name) : base(login, password, name) { }

            public static void ShowUsers()
            {
                Menu<User> usersMenu = new Menu<User>(users)
                {
                    Info = "Пользователи:",
                    Empty = "Пользователей нет",
                    ItemInstruction = (item) =>
                    {
                        if (item.GetType() == typeof(Admin))
                            Console.ForegroundColor = ConsoleColor.White;
                        else Console.ForegroundColor = ConsoleColor.DarkGray;
                        return item.Name + " (" + item.Login + ":" + item.Password + ")";
                    },
                    ColorInfo = new Tuple<ConsoleColor, string>[]
                    {
                        new Tuple<ConsoleColor, string>(ConsoleColor.White, "администратор"),
                        new Tuple<ConsoleColor, string>(ConsoleColor.DarkGray, "пользователь")
                    },
                    AddItemInstruction = AddUser
                };
                usersMenu.KeyInstructions += (key) =>
                {
                    if (key == ConsoleKey.Enter)
                        EditUser(users[usersMenu.SelectedItem]);
                    if (key == ConsoleKey.Tab)
                        usersMenu.AddItem();
                    if (key == ConsoleKey.Delete)
                    {
                        usersMenu.DeleteSelectedItem();
                    }
                };
                usersMenu.Help += String.Join("\n",
                    "Enter - редактировать",
                    "Tab - добавить",
                    "Delete - удалить",
                    "Backspace - назад");
                usersMenu.Use();
            }
            static void AddUser()
            {
                Console.WriteLine("Введите уникальный логин:");
                string login = Console.ReadLine().ToLower();
                if (users.Find(new Predicate<User>(x => x.Login == login)) != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Пользователь с таким логином уже существует");
                    Console.ResetColor();
                    Console.WriteLine("\nНажмите любую клавишу, чтобы продолжить");
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine("Введите пароль:");
                string password = Console.ReadLine();
                Console.WriteLine("Введите имя:");
                string name = Console.ReadLine();
                users.Add(new User(login, password, name));
            }
            static void EditUser(User user)
            {
                List<string> editUserMenuItems = new List<string>()
                {
                    "Изменить имя",
                    "Изменить логин",
                    "Изменить пароль",
                };
                Menu<string> editBookMenu = new Menu<string>(editUserMenuItems)
                {
                    Info = "Редактировать пользователя: \n\n " + user.Name + " (" + user.Login + ":" + user.Password + ")",
                    ItemInstruction = (item) =>
                    {
                        return item;
                    },
                };
                editBookMenu.KeyInstructions += (key) =>
                {
                    if (key == ConsoleKey.Enter)
                    {
                        if (editBookMenu.SelectedItem == 0)
                        {
                            Console.Clear();
                            Console.WriteLine("Введите имя:");
                            string name = Console.ReadLine();
                            user.Name = name;
                            editBookMenu.IsClosed = true;
                        }
                        if (editBookMenu.SelectedItem == 1)
                        {
                            Console.Clear();
                            Console.WriteLine("Введите логин:");
                            string login = Console.ReadLine();
                            user.Login = login;
                            editBookMenu.IsClosed = true;
                        }
                        if (editBookMenu.SelectedItem == 2)
                        {
                            Console.Clear();
                            Console.WriteLine("Введите пароль:");
                            string password = Console.ReadLine();
                            user.Password = password;
                            editBookMenu.IsClosed = true;
                        }
                    }
                };
                editBookMenu.Help += String.Join("\n",
                    "Enter - выбор",
                    "Backspace - назад");
                editBookMenu.Use();
            }
        }

        class Library
        {
            static Library()
            {
                books.Add(new Book("Лев Толстой", "Война и мир", "1869", true));
                books.Add(new Book("Джордж Оруэлл", "1984", "1949", true));
                books.Add(new Book("Джеймс Джойс", "Улисс", "1922", true));
                books.Add(new Book("Владимир Набоков", "Лолита", "1955", true));
                books.Add(new Book("Уильям Фолкнер", "Шум и ярость", "1929", true));
                books.Add(new Book("Ральф Эллисон", "Невидимка", "1952", true));
                books.Add(new Book("Вирджиния Вульф", "К маяку", "1927", true));
                books.Add(new Book("Джейн Остин", "Гордость и предубеждение", "1813", true));
                books.Add(new Book("Данте Алигьери", "Божественная комедия", "1321", true));
                books.Add(new Book("Джонатан Свифт", "Путешествия Гулливера", "1726", true));
                books.Add(new Book("Джордж Элиот", "Миддлмарч", "1874", true));
                books.Add(new Book("Чинуа Ачебе", "Распад", "1958", true));
                books.Add(new Book("Джером Д. Сэлинджер", "Над пропастью во ржи", "1951", true));
                books.Add(new Book("Маргарет Митчелл", "Маргарет Митчелл", "1936", true));
                books.Add(new Book("Габриэль Гарсия Маркес", "Сто лет одиночества", "1967", true));
                books.Add(new Book("Фрэнсис Скотт Фитцджеральд", "Великий Гэтсби", "1925", true));
                books.Add(new Book("Джозеф Хеллер", "Уловка-22", "1961", true));
                books.Add(new Book("Тони Моррисон", "Возлюбленная", "1987", true));
                books.Add(new Book("Джон Стейнбек", "Грозди гнева", "1939", true));
                books.Add(new Book("Салман Рушди", "Дети полуночи", "1981", true));
            }

            static List<Book> books = new List<Book>();

            public static void ShowBooks()
            {
                Menu<Book> booksMenu = new Menu<Book>(books)
                {
                    Info = "Библиотека:",
                    Empty = "В библиотеке нет книг",
                    ItemInstruction = (item) =>
                    {
                        if (item.IsAvailable)
                            Console.ForegroundColor = ConsoleColor.Green;
                        else Console.ForegroundColor = ConsoleColor.Red;
                        return item.Author + ", \"" + item.Name + "\" (" + item.Year + ")";
                    },
                    ColorInfo = new Tuple<ConsoleColor, string>[]
                    {
                        new Tuple<ConsoleColor, string>(ConsoleColor.Green, "доступные книги"),
                        new Tuple<ConsoleColor, string>(ConsoleColor.Red, "недоступные книги")
                    },
                    AddItemInstruction = AddBook
                };
                if (currentUser.GetType() == typeof(Admin))
                {
                    booksMenu.KeyInstructions += (key) =>
                    {
                        if (key == ConsoleKey.Enter)
                        {
                            if (books.Count != 0)
                            {
                                EditBook(books[booksMenu.SelectedItem]);
                            }
                        }
                        if (key == ConsoleKey.Tab)
                        {
                            booksMenu.AddItem();
                        }
                        if (key == ConsoleKey.Delete)
                        {
                            booksMenu.DeleteSelectedItem();
                        }
                    };
                    booksMenu.Help += String.Join("\n",
                    "Enter - редактировать",
                    "Tab - добавить",
                    "Delete - удалить",
                    "Backspace - назад");
                }
                else if (currentUser.GetType() == typeof(User))
                {
                    booksMenu.KeyInstructions += (key) =>
                    {
                        if (key == ConsoleKey.Enter)
                        {
                            currentUser.TakeBook(books[booksMenu.SelectedItem]);
                            books[booksMenu.SelectedItem].IsAvailable = false;
                        }
                    };
                    booksMenu.Help += String.Join("\n",
                    "Enter - взять книгу",
                    "Backspace - назад");
                }
                booksMenu.Use();
            }
            static void AddBook()
            {
                Console.WriteLine("Автор:");
                string author = Console.ReadLine();
                Console.WriteLine("Название:");
                string name = Console.ReadLine();
                Console.WriteLine("Год:");
                string year = Console.ReadLine();
                books.Add(new Book(name, author, year, true));
            }
            static void EditBook(Book book)
            {
                List<string> editBookMenuItems = new List<string>()
                {
                    "Изменить автора",
                    "Изменить название",
                    "Изменить год",
                };
                Menu<string> editBookMenu = new Menu<string>(editBookMenuItems)
                {
                    Info = "Редактировать книгу: \n\n " + book.Author + ", \"" + book.Name + "\" (" + book.Year + ")",
                    ItemInstruction = (item) =>
                    {
                        return item;
                    },
                };
                editBookMenu.KeyInstructions += (key) =>
                {
                    if (key == ConsoleKey.Enter)
                    {
                        if (editBookMenu.SelectedItem == 0)
                        {
                            Console.Clear();
                            Console.WriteLine("Введите автора:");
                            string author = Console.ReadLine();
                            book.Author = author;
                            editBookMenu.IsClosed = true;
                        }
                        if (editBookMenu.SelectedItem == 1)
                        {
                            Console.Clear();
                            Console.WriteLine("Введите название:");
                            string name = Console.ReadLine();
                            book.Name = name;
                            editBookMenu.IsClosed = true;
                        }
                        if (editBookMenu.SelectedItem == 2)
                        {
                            Console.Clear();
                            Console.WriteLine("Введите год:");
                            string year = Console.ReadLine();
                            book.Year = year;
                            editBookMenu.IsClosed = true;
                        }
                    }
                };
                editBookMenu.Help += String.Join("\n",
                    "Enter - выбор",
                    "Backspace - назад");
                editBookMenu.Use();
            }
            public static void MakeBookAvailable(Book book)
            {
                book.IsAvailable = true;
            }
        }
        class Book
        {
            public Book(string author, string name, string year, bool isAvailable)
            {
                Author = author;
                Name = name;
                Year = year;
                IsAvailable = isAvailable;
            }

            public string Author { get; set; }
            public string Name { get; set; }
            public string Year { get; set; }
            public bool IsAvailable { get; set; }
        }
    }
}
