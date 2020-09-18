using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Books.API.Services
{
    public interface IBooksRepository
    {
        Entities.Book GetBook(Guid id);
        
        IEnumerable<Entities.Book> GetBooks();

        Task<Entities.Book> GetBookAsync(Guid id);
        
        Task<IEnumerable<Entities.Book>> GetBooksAsync();
    }
}