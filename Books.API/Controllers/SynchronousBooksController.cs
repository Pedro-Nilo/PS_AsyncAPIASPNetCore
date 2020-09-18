using System;
using Books.API.Services;
using Microsoft.AspNetCore.Mvc;


namespace Books.API.Controllers
{
    [ApiController]
    [Route("api/synchronousbooks")]
    public class SynchronousBooksController : ControllerBase
    {
        private readonly IBooksRepository _booksRepository;


        public SynchronousBooksController(IBooksRepository booksRepository)
        {
            _booksRepository = booksRepository ?? throw new ArgumentNullException(nameof(booksRepository));
        }


        [HttpGet]
        [Route("{id}")]
        public IActionResult GetBook(Guid id)
        {
            var bookEntity = _booksRepository.GetBook(id);

            if(bookEntity == null)
            {
                return NotFound();
            }

            return Ok(bookEntity);
        }

        [HttpGet]
        public IActionResult GetBooks()
        {
            var bookEntities = _booksRepository.GetBooks();

            return Ok(bookEntities);
        }
    }
}