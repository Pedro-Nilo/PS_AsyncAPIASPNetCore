using System;
using System.Threading.Tasks;
using AutoMapper;
using Books.API.Filters;
using Books.API.Models;
using Books.API.Services;
using Microsoft.AspNetCore.Mvc;


namespace Books.API.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly IBooksRepository _booksRepository;
        private readonly IMapper _mapper;


        public BooksController(IBooksRepository booksRepository,
            IMapper mapper)
        {
            _booksRepository = booksRepository ??
                throw new ArgumentNullException(nameof(booksRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }


        [HttpPost]
        [BookResultFilter]
        public async Task<IActionResult> CreateBook(BookForCreation bookForCreation)
        {
            var bookEntity = _mapper.Map<Entities.Book>(bookForCreation);

            _booksRepository.AddBook(bookEntity);

            await _booksRepository.SaveChangesAsync();
            await _booksRepository.GetBookAsync(bookEntity.Id);

            return CreatedAtRoute(
                "GetBook",
                new { id = bookEntity.Id },
                bookEntity);
        }

        [HttpGet]
        [Route("{id}", Name = "GetBook")]
        [BookResultFilter()]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var bookEntity = await _booksRepository.GetBookAsync(id);

            if(bookEntity == null)
            {
                return NotFound();
            }

            var bookCover = await _booksRepository.GetBookCoverAsync("dummycover");

            return Ok(bookEntity);
        }

        [HttpGet]
        [BooksResultFilter()]
        public async Task<IActionResult> GetBooks()
        {
            var bookEntities = await _booksRepository.GetBooksAsync();

            return Ok(bookEntities);
        }
    }
}