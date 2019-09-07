using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using one = Xyqlx.OneNote;

namespace OneNoteTagGUI
{
    class PageBrowser : INotifyPropertyChanged
    {
        private one.Section section;
        private one.PageInfo[] pageInfos;
        private one.TagPage current;
        private ObservableCollection<string> tags;
        private int index = 0;
        List<string> commonTags;

        public event PropertyChangedEventHandler PropertyChanged;

        //选中无页面的节会出异常（
        public PageBrowser(one.Section section)
        {
            this.section = section;
            pageInfos = section.PageInfos.ToArray();
            current = new one.TagPage(one.App.GetPage(pageInfos[0].ID));
            tags = new ObservableCollection<string>(current.tags);
            commonTags = new List<string>();
        }

        public void ClacCommonTags()
        {
            var client = new MongoClient("mongodb://127.0.0.1:27017");
            var database = client.GetDatabase("xyals");
            var collection = database.GetCollection<BsonDocument>("medium");
            var query = collection.Find(new BsonDocument()).Project(Builders<BsonDocument>.Projection.Include("tags").Exclude("_id"));
            var cursor = query.ToCursor();
            Dictionary<string, int> tagCnt = new Dictionary<string, int>();
            while(cursor.MoveNext())
                foreach (var doc in cursor.Current)
                    foreach (var tag in doc["tags"] as BsonArray)
                        tagCnt[tag.ToString()] = tagCnt.ContainsKey(tag.ToString()) ? tagCnt[tag.ToString()] + 1 : 0;
            commonTags = tagCnt.Keys.OrderByDescending(x => tagCnt[x]).Take(20).ToList();
        }

        public void MoveToPrevious()
        {
            if (index != 0)
                index--;
            MoveToIndex();
        }
        public void MoveToNext()
        {
            if (index != pageInfos.Length - 1)
                index++;
            MoveToIndex();
        }
        public void MoveToTop()
        {
            index = 0;
            MoveToIndex();
        }
        public void MoveToBottom()
        {
            index = pageInfos.Length - 1;
            MoveToIndex();
        }

        public void MoveToIndex()
        {
            if (index < 0 || index >= pageInfos.Length)
                return;
            current = new one.TagPage(one.App.GetPage(pageInfos[index].ID));
            tags = new ObservableCollection<string>(current.tags);
            OnPropertyChanged();
        }

        protected void OnPropertyChanged(string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public void AddTags(IEnumerable<string> other)
        {
            current.tags.UnionWith(other.Where(x => x != ""));
            current.Update();
            tags.Clear();
            foreach (var s in current.tags)
                tags.Add(s);
            Task.Run(() => UpdateTags(current.Name, current.DateTime, current.tags));
        }
        public void RemoveTag(string other)
        {
            current.tags.Remove(other);
            current.Update();
            tags.Clear();
            foreach (var s in current.tags)
                tags.Add(s);
            Task.Run(() => UpdateTags(current.Name, current.DateTime, current.tags));
        }

        public void UpdateTags(string name, DateTime dateTime, IEnumerable<string> tags)
        {
            //连接数据库
            var updateBson = new BsonDocument {
                {"name",name },
                //{ "time",dateTime },
                {"tags",new BsonArray(tags) }
            };
            var client = new MongoClient("mongodb://127.0.0.1:27017");
            var database = client.GetDatabase("xyals");
            var collection = database.GetCollection<BsonDocument>("medium");
            collection.UpdateOne(Builders<BsonDocument>.Filter.Eq("time", dateTime), new BsonDocument { { "$set", updateBson } }, new UpdateOptions { IsUpsert = true });
        }

        public string Name => current.Name;
        public string Content => current.PlainText;
        public ObservableCollection<string> Tags => tags;
        public int PageIndex { get => index + 1; set { index = value - 1;MoveToIndex(); } }

        public List<string> CommonTags { get => commonTags; }
    }
}
