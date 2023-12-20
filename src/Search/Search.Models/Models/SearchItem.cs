namespace Search.Models
{
    public record SearchItem
    {
        public SearchItem(string documentExtensions, string image, string content, DateTime created, string title, string index, float maxScore, int position, double score, string type, string id, float absScore)
        {
            DocumentExtensions = documentExtensions;
            Image = image;
            Content = content;
            Created = created;
            Title = title;
            Index = index;
            MaxScore = maxScore;
            Position = position;
            Score = score;
            Type = type;
            Id = id;
            AbsScore = absScore;
        }

        public string DocumentExtensions { get; set; }
        public string Image { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public string Title { get; set; }
        public string Index { get; set; }
        public float MaxScore { get; set; }
        public int Position { get; set; }
        public double Score { get; set; }
        public string Type { get; set; }
        public string Id { get; set; }
        public float AbsScore { get; set; }
        public bool IsPage { get; set; } = false;
        public Maybe<string> PageUrl { get; set; }
    }

    public record SearchItem<T> : SearchItem
    {
        public SearchItem(string documentExtensions, string image, string content, DateTime created, string title, string index, float maxScore, int position, double score, string type, string id, float absScore, T data) : base(documentExtensions, image, content, created, title, index, maxScore, position, score, type, id, absScore)
        {
            Data = data;
        }

        public SearchItem(T data, SearchItem searchItem) : base(searchItem)
        {
            Data = data;
        }

        public T Data { get; set; }
    }
}
