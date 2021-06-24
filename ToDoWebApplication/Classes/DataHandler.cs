using System;
using System.Collections.Generic;
using System.Linq;
using ToDoWebApplication.Models;
using System.IO;

namespace ToDoWebApplication.Classes
{
    static public class DataHandler
    {
        //получить все задачи
        static public List<Task> GetAllTasks(string databasePath)
        {
            List<Task> tasks = new List<Task>();

            try
            {
                //построчное чтение
                using (StreamReader sr = new StreamReader(databasePath, System.Text.Encoding.GetEncoding(1251)))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        Task task = new Task();

                        //разделить строку на составляющие по запятым
                        string[] data = line.Split(',');

                        //пропустить первую строку
                        if (data[0] == "id")
                            continue;

                        //пропустить пустые строки
                        if (data.Length != 6)
                            continue;

                        task.Id = Convert.ToInt32(data[0]);
                        task.Name = data[1];
                        task.Description = data[2];
                        task.StartDate = Convert.ToDateTime(data[3]);
                        task.EndDate = Convert.ToDateTime(data[4]);
                        task.File = data[5];

                        tasks.Add(task);
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return tasks;
        }

        //записать задачи
        static public void SaveTasks(string databasePath, List<Task> tasks)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(databasePath, false, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine("id,name,description,startdate,enddate,file");

                    foreach (Task task in tasks)
                        sw.WriteLine(GetLineForDatabase(task));
                }
            }
            catch (Exception ex)
            {
                string exception = ex.ToString();
            }
        }

        //получить задачу по ID
        static public Task GetTaskByID(int id, string dbPath)
        {
            List<Task> tasks = GetAllTasks(dbPath);

            foreach (Task tsk in tasks)
                if (tsk.Id == id)
                {
                    Task task = new Task();

                    task.Id = tsk.Id;
                    task.Name = tsk.Name;
                    task.Description = tsk.Description;
                    task.StartDate = tsk.StartDate;
                    task.EndDate = tsk.EndDate;
                    task.File = tsk.File;

                    return task;
                }

            return null;
        }

        //редактировать задачу по ID
        static public void EditTask(int id, string dbPath, string name, string description, DateTime enddate)
        {
            List<Task> tasks = GetAllTasks(dbPath);

            foreach (Task task in tasks)
            {
                if (task.Id == id)
                {
                    task.Name = name;
                    task.Description = description;
                    task.EndDate = enddate;
                }
            }

            SaveTasks(dbPath, tasks);
        }
        
        //удалить задачу по ID
        static public void DeleteTask(int id, string dbPath)
        {
            List<Task> tasks = GetAllTasks(dbPath);

            int index = -1;
            //найти индекс удаляемого элемента
            for (int i = 0; i < tasks.Count; i++) 
                if (tasks[i].Id == id)
                {
                    index = i;
                    break;
                }

            if (index > -1)
                tasks.RemoveAt(index);

            SaveTasks(dbPath, tasks);
        }

        //добавить задачу
        static public void AddTask(Task task, string dbPath)
        {
            List<Task> tasks = GetAllTasks(dbPath);
            task.Id = GetNewID(dbPath);
            tasks.Add(task);
            SaveTasks(dbPath, tasks);
        }

        //сгенерировать ID для новой задачи
        static private int GetNewID(string dbPath)
        {
            List<Task> tasks = GetAllTasks(dbPath);
            if (tasks.Count == 0)
                return 0;
            else 
                return tasks.Last().Id + 1;
        }

        //сформировать из задачи строку
        static private string GetLineForDatabase(Task task)
        {
            string line = "";

            line = line + task.Id.ToString() + ',' + task.Name + ',' + task.Description + ',' 
                            + task.StartDate.ToString("dd.MM.yyyy") + ',' + task.EndDate.ToString("dd.MM.yyyy") 
                            + ',' + task.File;

            return line;
        }

        //выполнение поиска
        static public List<Task> SearchTasks(string query, string dbPath)
        {
            //перевести поисковой запрос в нижний регистр
            query = query.ToLower();

            //слова поискового запроса
            string[] queryWords = query.Split(' ');

            //полный список задач
            List<Task> tasks = GetAllTasks(dbPath);

            //соответствующие запросу
            List<Task> filteredTasks = new List<Task>();

            foreach(Task task in tasks)
            {
                //соответствует запросу
                bool flag = false;

                //есть ли слова запроса в названии или описании
                for (int i = 0; i < queryWords.Length; i++)
                {
                    if (task.Name.ToLower().Contains(queryWords[i]))
                    {
                        flag = true;
                        break;
                    }
                    if (task.Description.ToLower().Contains(queryWords[i]))
                    {
                        flag = true;
                        break;
                    }
                }

                //соответствует - переносится в результат
                if (flag == true)
                    filteredTasks.Add(task);
            }

            return filteredTasks;
        }
    }
}