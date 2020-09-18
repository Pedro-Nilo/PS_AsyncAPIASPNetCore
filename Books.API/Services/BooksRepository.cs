using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Books.API.Contexts;
using Books.API.Entities;
using Microsoft.EntityFrameworkCore;


namespace Books.API.Services
{
    public class BooksRepository : IBooksRepository, IDisposable
    {
        private BooksContext _context;


        public BooksRepository(BooksContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


        public Book GetBook(Guid id)
        {
            return _context.Books
                .Include(book => book.Author)
                .FirstOrDefault(book => book.Id == id);
        }

        public IEnumerable<Book> GetBooks()
        {
            _context.Database.ExecuteSqlRaw("SELECT pg_sleep(2);");
            return _context.Books
                .Include(book => book.Author)
                .ToList();
        }
        
        public async Task<Book> GetBookAsync(Guid id)
        {
            return await _context.Books
                .Include(book => book.Author)
                .FirstOrDefaultAsync(book => book.Id == id);
        }

        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT pg_sleep(2);");
            return await _context.Books
                .Include(book => book.Author)
                .ToListAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }
    }
}