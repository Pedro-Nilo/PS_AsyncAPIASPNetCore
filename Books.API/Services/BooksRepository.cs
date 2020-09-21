using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Books.API.Contexts;
using Books.API.Entities;
using Books.API.ExternalModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Books.API.Services
{
    public class BooksRepository : IBooksRepository, IDisposable
    {
        private BooksContext _context;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ILogger<BooksRepository> _logger;

        private CancellationTokenSource _cancellationTokenSource;
        

        public BooksRepository(BooksContext context,
            IHttpClientFactory httpClientFactory,
            ILogger<BooksRepository> logger)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
            _httpClientFactory = httpClientFactory ??
                throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public void AddBook(Book bookToAdd)
        {
            if (bookToAdd == null)
            {
                throw new ArgumentNullException(nameof(bookToAdd));
            }

            _context.Add(bookToAdd);
        }

        private async Task<BookCover> DownloadBookCoverAsync(
            HttpClient httpClient, string bookCoverUrl,
            CancellationToken cancellationToken)
        {
            var response = await httpClient
                .GetAsync(bookCoverUrl, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var bookCover = JsonSerializer.Deserialize<BookCover>(
                    await response.Content.ReadAsStringAsync(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    });

                return bookCover;
            }

            _cancellationTokenSource.Cancel();
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

        // private Task<int> GetBookPages()
        // {
        //     Task.Run must be avoid on server side applications
        //     return Task.Run(() =>
        //     {
        //         var pageCalculator = new Books.Legacy.ComplicatedPageCalculator();

        //         _logger.LogInformation($"ThreadId when calculating the amount of pages: " +
        //             $"{System.Threading.Thread.CurrentThread.ManagedThreadId}");

        //         return pageCalculator.CalculateBookPages();
        //     });
        // }
        
        public async Task<Book> GetBookAsync(Guid id)
        {
            // _logger.LogInformation($"ThreadId when entering GetBookAsync: " +
            //         $"{System.Threading.Thread.CurrentThread.ManagedThreadId}");

            // var bookPages = await GetBookPages();

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
            
            if (response.IsSuccessStatusCode)
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

            _cancellationTokenSource = new CancellationTokenSource();

            var bookCoverUrls = new []
            {
                $"http://127.0.0.1:5050/api/bookcovers/{bookId}-dummycover1",
                //$"http://127.0.0.1:5050/api/bookcovers/{bookId}-dummycover2?returnFault=true",
                $"http://127.0.0.1:5050/api/bookcovers/{bookId}-dummycover2",
                $"http://127.0.0.1:5050/api/bookcovers/{bookId}-dummycover3",
                $"http://127.0.0.1:5050/api/bookcovers/{bookId}-dummycover4",
                $"http://127.0.0.1:5050/api/bookcovers/{bookId}-dummycover5"
            };

            var downloadBookCoverTasksQuery = 
                from bookCoverUrl
                in bookCoverUrls
                select DownloadBookCoverAsync(httpClient, bookCoverUrl,
                    _cancellationTokenSource.Token);

            var downloadBookCoverTasks = downloadBookCoverTasksQuery.ToList();

            try
            {
                return await Task.WhenAll(downloadBookCoverTasks);
            }
            catch (OperationCanceledException operationCanceledException)
            {
                _logger.LogInformation($"{operationCanceledException.Message}");

                foreach (var task in downloadBookCoverTasks)
                {
                    _logger.LogInformation($"Task {task.Id} has status {task.Status}");
                }

                return new List<BookCover>();
            }
            catch (Exception exception)
            {
                _logger.LogInformation($"{exception.Message}");
                throw;
            }
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

                if(_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
            }
        }
    }
}