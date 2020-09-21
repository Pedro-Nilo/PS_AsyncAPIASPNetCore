using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Books.API.Contexts;
using Books.API.Entities;
using Books.API.ExternalModels;
using Microsoft.EntityFrameworkCore;


namespace Books.API.Services
{
    public class BooksRepository : IBooksRepository, IDisposable
    {
        private BooksContext _context;

        private readonly IHttpClientFactory _httpClientFactory;
        

        public BooksRepository(BooksContext context,
            IHttpClientFactory httpClientFactory)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
            _httpClientFactory = httpClientFactory ??
                throw new ArgumentNullException(nameof(httpClientFactory));
        }


        public void AddBook(Book bookToAdd)
        {
            if(bookToAdd == null)
            {
                throw new ArgumentNullException(nameof(bookToAdd));
            }

            _context.Add(bookToAdd);
        }

        private async Task<BookCover> DownloadBookCoverAsync(
            HttpClient httpClient, string bookCoverUrl)
        {
            var response = await httpClient.GetAsync(bookCoverUrl);

            if(response.IsSuccessStatusCode)
            {
                var bookCover = JsonSerializer.Deserialize<BookCover>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    });

                return bookCover;
            }

            return null;
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

        public async Task<IEnumerable<Book>> GetBooksAsync(IEnumerable<Guid> bookIds)
        {
            return await _context.Books.Where(book => bookIds.Contains(book.Id))
                .Include(book => book.Author).ToListAsync();
        }

        public async Task<BookCover> GetBookCoverAsync(string coverId)
        {
            var httpClient = _httpClientFactory.CreateClient("HttpClient");
            var response = await httpClient
                .GetAsync($"http://127.0.0.1:5050/api/bookcovers/{coverId}");
            
            if(response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<BookCover>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    });
            }

            return null;
        }

        public async Task<IEnumerable<BookCover>> GetBookCoversAsync(Guid bookId)
        {
            var httpClient = _httpClientFactory.CreateClient("HttpClient");
            var bookCovers = new List<BookCover>();
            var bookCoverUrls = new []
            {
                $"http://127.0.0.1:5050/api/bookcovers/{bookId}-dummycover1",
                $"http://127.0.0.1:5050/api/bookcovers/{bookId}-dummycover2",
                $"http://127.0.0.1:5050/api/bookcovers/{bookId}-dummycover3",
                $"http://127.0.0.1:5050/api/bookcovers/{bookId}-dummycover4",
                $"http://127.0.0.1:5050/api/bookcovers/{bookId}-dummycover5"
            };

            var downloadBookCoverTasksQuery = 
                from bookCoverUrl
                in bookCoverUrls
                select DownloadBookCoverAsync(httpClient, bookCoverUrl);

            var downloadBookCoverTasks = downloadBookCoverTasksQuery.ToList();

            return await Task.WhenAll(downloadBookCoverTasks);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
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