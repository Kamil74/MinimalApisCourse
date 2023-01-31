using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Library.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Xunit;

namespace Library.Api.Tests.Integration;

public class LibraryEndpointsTests
: IClassFixture<WebApplicationFactory<IApiMarker>>
{
    private readonly WebApplicationFactory<IApiMarker> _factory;

    public LibraryEndpointsTests(WebApplicationFactory<IApiMarker> factory)
    {
        _factory = factory;
    }

   /* [Fact]
    public void Test()
    {
        var httpClient = _factory.CreateClient();
    }*/
   [Fact]
   public async Task CreateBook_CreateBook_WhenDataIsCorrrect()
   {
       // Arrange
       var httpClient = _factory.CreateClient();
       var book = GenerateBook();
       
       
       // Act
       var result = await httpClient.PostAsJsonAsync("/books", book);
       var createdBook = await result.Content.ReadFromJsonAsync<Book>();

       // Assert

       result.StatusCode.Should().Be(HttpStatusCode.Created);
       createdBook.Should().BeEquivalentTo(book);
       result.Headers.Location.Should().Be($"/books/{book.Isbn}");
   }

   [Fact]
   public async Task CreateBook_Fails_WhenIsbnIsInvalid()
   {
       // Arrange
       var httpClient = _factory.CreateClient();
       var book = GenerateBook();
       book.Isbn = "INVALID";
       


       //Act
       var result = await httpClient.PostAsJsonAsync("/books", book);
       var errors = await result.Content.ReadFromJsonAsync < IEnumerable<ValidationError>>();
       var error = errors!.Single();

       //Assert
       result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
       error.PropertyName.Should().Be("Isbn");
       error.ErrorMessage.Should().Be("Value was not valid ISBN-13");
       
   }
   [Fact]
   public async Task CreateBook_Fails_WhenBookExists()
   {
       // Arrange
       var httpClient = _factory.CreateClient();
       var book = GenerateBook();
       
       


       //Act
        await httpClient.PostAsJsonAsync("/books", book);
       var result = await httpClient.PostAsJsonAsync("/books", book);
       var errors = await result.Content.ReadFromJsonAsync < IEnumerable<ValidationError>>();
       var error = errors!.Single();

       //Assert
       result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
       error.PropertyName.Should().Be("Isbn");
       error.ErrorMessage.Should().Be("A book with this ISBN-13 already exists");
       
   }


   private Book GenerateBook(string title = "The Dirty Coder")
   {
       return new Book
       {
           Isbn = GenerateIsbn(),
           Title = title,
           Author = "Rafael Milewski",
           PageCount = 420,
           ShortDescription = "All my tricks in one book",
           ReleaseDate = new DateTime(2023, 01, 30)
       };
   }

   private string GenerateIsbn()
   {
       return $"{Random.Shared.Next(100, 999)}-" +
              $"{Random.Shared.Next(1000000000, 2100999999)}";
   }
}

