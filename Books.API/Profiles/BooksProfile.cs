using System.Collections.Generic;
using AutoMapper;


namespace Books.API.Profiles
{
    public class BooksProfile : Profile
    {
        public BooksProfile()
        {
            CreateMap<Entities.Book, Models.Book>()
                .ForMember(destination => destination.Author, options => options.MapFrom(source =>
                    $"{source.Author.FirstName} {source.Author.LastName}"));

            CreateMap<Models.BookForCreation, Entities.Book>();

            CreateMap<Entities.Book, Models.BookWithCovers>()
                .ForMember(destination => destination.Author, options => options.MapFrom(source =>
                    $"{source.Author.FirstName} {source.Author.LastName}"));

            CreateMap<IEnumerable<ExternalModels.BookCover>, Models.BookWithCovers>()
                .ForMember(destination => destination.BookCovers, options => options.MapFrom(source =>
                    source));

            CreateMap<ExternalModels.BookCover, Models.BookCover>();
        }
    }
}