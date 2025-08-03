namespace BookStoreMVC.Models
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public void AddItem(BookDto book, int quantity = 1)
        {
            var existingItem = Items.FirstOrDefault(i => i.BookId == book.Id);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                Items.Add(new CartItem
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Price = book.Price,
                    ImageUrl = book.ImageUrl,
                    Quantity = quantity
                });
            }
        }

        public void RemoveItem(int bookId)
        {
            Items.RemoveAll(i => i.BookId == bookId);
        }

        public void UpdateQuantity(int bookId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.BookId == bookId);
            if (item != null)
            {
                if (quantity <= 0)
                    RemoveItem(bookId);
                else
                    item.Quantity = quantity;
            }
        }

        public decimal GetTotalPrice()
        {
            return Items.Sum(i => i.TotalPrice);
        }

        public int GetTotalQuantity()
        {
            return Items.Sum(i => i.Quantity);
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}