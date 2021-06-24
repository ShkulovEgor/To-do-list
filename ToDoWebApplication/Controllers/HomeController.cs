using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ToDoWebApplication.Classes;
using ToDoWebApplication.Models;

namespace ToDoWebApplication.Controllers
{
    public class HomeController : Controller
    {
        static private string dbPath;

        /*Главная страница*/
        public ActionResult Index()
        {
            dbPath = Server.MapPath("~/Data/database.csv");

            List<Task> tasks = DataHandler.GetAllTasks(dbPath);

            if (tasks == null)
            {
                ViewBag.Error = "Не найдено задач";
                tasks = new List<Task>();
            }
            else
                ViewBag.Tasks = tasks;

            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"].ToString();

            return View();
        }

        /*Добавить задачу*/
        [HttpGet]
        public ActionResult AddTask()
        {
            return View();
        }

        /*Сохранение добавленной задачи*/
        [HttpPost]
        public ActionResult SaveTask(HttpPostedFileBase upload, string name, string description, DateTime enddate)
        {
            try
            {
                if ((name == "") || (description == "") || (enddate == Convert.ToDateTime("01.01.2021")))
                {
                    TempData["Error"] = "Были введены не все поля";
                    return RedirectToAction("Index");
                }
                else
                {
                    DateTime startdate = DateTime.Now;

                    string dbPath = Server.MapPath("~/Data/database.csv");

                    //получение имени файла
                    string filename = Path.GetFileName(upload.FileName);
                    //сохранение файла на сервер
                    upload.SaveAs(Server.MapPath("~/Files/" + filename));

                    Task task = new Task();
                    task.Name = name;
                    task.Description = description;
                    task.StartDate = startdate;
                    task.EndDate = enddate;
                    task.File = filename;
                    DataHandler.AddTask(task, dbPath);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Не удалось загрузить файл: " + ex.ToString();
            }

            return RedirectToAction("Index");
        }

        /*Скачивание файла*/
        [HttpGet]
        public FileResult Download(string id)
        {
            //получение файла по идентификатору
            string filePath = DataHandler.GetTaskByID(Convert.ToInt32(id), dbPath).File;

            //загрузка файла
            return GetFile(filePath);
        }

        /*Получение файла для скачивания из хранилища на сервере*/
        private FileResult GetFile(string filePath)
        {
            try
            {
                //путь к файлу
                string filepath = Server.MapPath("~/Files/" + filePath);
                //тип файла
                string filetype = "application/pdf";
                //имя файла
                string filename = filePath;

                return File(filepath, filetype, filename);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Не удалось загрузить файл: " + ex.ToString();
                return null;
            }
        }

        /*Редактирование информации о выбранном файле*/
        [HttpGet]
        public ActionResult Edit(string id)
        {
            if (id != null)
            {
                try
                {
                    //найти задачу по идентификатору
                    Task task = DataHandler.GetTaskByID(Convert.ToInt32(id), dbPath);

                    //передать задачу в представление
                    ViewBag.Task = task;
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Произошла ошибка: " + ex.ToString();
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["Error"] = "Пустой идентификатор.";
                return RedirectToAction("Index");
            }

            return View();
        }

        /*Сохранение изменения информации о задаче*/
        [HttpPost]
        public ActionResult SaveChanges(string id, string name, string description, DateTime enddate)
        {
            if (id != null)
            {
                try
                {
                    if ((name == "") || (description == "") || (enddate == Convert.ToDateTime("01.01.2021")))
                    {
                        TempData["Error"] = "Были введены не все поля";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        //отредактировать задачу
                        DataHandler.EditTask(Convert.ToInt32(id), dbPath, name, description, enddate);

                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Произошла ошибка: " + ex.ToString();
                    return RedirectToAction("Index");
                }
            }
            else
            {
                TempData["Error"] = "Пустой идентификатор.";
                return RedirectToAction("Index");
            }

        }

        /*Удаление задачи*/
        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (id != null)
            {
                try
                {
                    //нахождение файла задачи
                    string filename = DataHandler.GetTaskByID(Convert.ToInt32(id), dbPath).File;

                    //путь к файлу
                    string filepath = Server.MapPath("~/Files/" + filename);

                    //нахождение файла на хранилище сервера
                    FileInfo fileInfo = new FileInfo(filepath);

                    if (fileInfo.Exists)
                    {
                        //удаление на сервере
                        fileInfo.Delete();
                    }

                    //удаление задачи
                    DataHandler.DeleteTask(Convert.ToInt32(id), dbPath);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Произошла ошибка: " + ex.ToString();
                }
            }
            else
            {
                TempData["Error"] = "Произошла ошибка.";
            }

            return RedirectToAction("Index");
        }

        /*Выполнение поиска*/
        [HttpGet]
        public ActionResult Search(string query)
        {
            if (query != null)
            {
                //все задачи, удовлетворяющие запросу
                List<Task> tasks = DataHandler.SearchTasks(query, dbPath);

                //передать результат в представление
                if (tasks.Count != 0)
                {
                    ViewBag.Tasks = tasks;
                }

                return View();
            }
            else
            {
                TempData["Error"] = "Пустой запрос.";
                return RedirectToAction("Index");
            }
        }

    }
}