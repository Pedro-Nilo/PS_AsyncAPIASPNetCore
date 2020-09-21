using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Books.API.ExternalModels;

namespace Books.API.Services
{
    public interface IBooksRepository
    {
        void AddBook(Entities.Book bookToAdd);

        Entities.Book GetBook(Guid id);
        
        IEnumerable<Entities.Book> GetBooks();

        Task<Entities.Book> GetBookAsync(Guid id);
        
        Task<IEnumerable<Entities.Book>> GetBooksAsync();

        Task<IEnumerable<Entities.Book>> GetBooksAsync(IEnumerable<Guid> bookIds);

        Task<BookCover> GetBookCoverAsync(string coverId);

        Task<IEnumerable<BookCover>> GetBookCoversAsync(Guid bookId);

        Task<bool> SaveChangesAsync();
    }
}