using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ToDoWebAPI
{
    public class DashBoardDTO
    {
        public int numTaskToday { get; set; }
        public List<NotDoneItemDTO> notDoneTasks { get; set; }
        public List<ItemLists> lists { get; set; }
    }
    public class NotDoneItemDTO
    {
        public int countNotDoneTask { get; set; }
        public int idList { get; set; }
    }
    public class ItemLists
    {
        public string name { get; set; }
        public int id { get; set; }
    }

    public class ToDoItemService
    {
        private ToDoItemContext _context;
        public ToDoItemService(ToDoItemContext context)
        {
            this._context = context;
        }
        public List<Item> GetAllTaskByIdList(int idList)
        {
            return _context.list_items.Where(items => items.taskListId == idList).ToList();
        }
        public List<Item> GetAllTaskByIdListWithAll(int idList, bool all)
        {
            if (all)
            {
                return _context.list_items.Where(items => items.taskListId == idList).ToList();
            }
            else
            {
                return _context.list_items.Where(items => items.taskListId == idList).Where(items => items.done == false).ToList();
            }

        }
        public List<Item> GetAllNotDoneTask()
        {
            return _context.list_items.Where(items => items.done == false).ToList();
        }
        public List<Item> GetItemsByListId(int listId)
        {
            return _context.list_items.Where(items => items.taskListId == listId).ToList();
        }
        public List<Item> GetAllTasks()
        {
            return _context.list_items.ToList();
        }
        public List<TaskList> GetAllLists()
        {
            return _context.task_lists.ToList();
        }
        public DashBoardDTO GetDashboard()
        {
            int numTaskToday;
            numTaskToday = _context.list_items.Where(i => i.dueDate.Value.Date == DateTime.Now.Date).Count();
            var countNotDoneTaskByName =
                (from item in _context.Set<Item>().Where(i => i.done == false)
                 join list in _context.Set<TaskList>()
                     on item.taskListId equals list.taskListId
                 group item by new { list.name, list.taskListId } into g
                 select new NotDoneItemDTO { countNotDoneTask = g.Count(), idList = g.Key.taskListId }).ToList();
            var lists = (from taskLists in _context.Set<TaskList>()
                    select new ItemLists { name = taskLists.name, id = taskLists.taskListId }).ToList();
            return new DashBoardDTO() { notDoneTasks = countNotDoneTaskByName, numTaskToday = numTaskToday,lists = lists };
        }

        public List<Item> GetTodayTask()
        {
            return _context.list_items
                .Where(i => i.dueDate.Value.Date <= DateTime.Now.Date)
                .Where(i => i.done == false)
                .Include(i => i.TaskList)
                .ToList();
        }

        public Item CreateToDoItemInList(int idList, Item item)
        {
            item.taskListId = idList;
            _context.Add(item);
            _context.SaveChanges();
            return item;
        }

        public TaskList CreateToDoItemList(TaskList taskList)
        {
            _context.task_lists.Add(taskList);
            _context.SaveChanges();
            return taskList;
        }
        public List<Item> GetById(int idList, int id)
        {
            var items = GetAllTaskByIdList(idList);
            var item = items.Where(item => item.itemId == id).ToList();
            return item;
        }
        public List<Item> GetItemWithoutListId(int id)
        {
            var items = GetAllTasks();
            var item = items.Where(item => item.itemId == id).ToList();
            return item;
        }
        public Item PutItem(int idList, int id, Item item)
        {
            item.taskListId = idList;
            item.itemId = id;
            _context.list_items.Update(item);
            _context.SaveChanges();
            return item;
        }

        public Item PatchItemWithoutId(Item item)
        {
            _context.list_items.Update(item);
            _context.SaveChanges();
            return item;
        }

        public void DeleteItem(int idList, int id)
        {
            Item item = new Item() { itemId = id, taskListId = idList };
            _context.list_items.Remove(item);
            _context.SaveChanges();
        }
        public void DeleteItemWithoutListID(int id)
        {
            Item item = new Item() { itemId = id };
            _context.list_items.Remove(item);
            _context.SaveChanges();
        }

        public void DeleteTaskList(int idList)
        {
            TaskList taskList = new TaskList() { taskListId = idList };
            _context.task_lists.Remove(taskList);
            _context.SaveChanges();
        }
    }
}