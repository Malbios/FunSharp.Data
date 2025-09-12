namespace FunSharp.Data.Test

open System
open System.IO
open FunSharp.Data
open Xunit
open Faqt
open Faqt.Operators

type TestModel = {
    Id: Guid
    Text: string
    Number: int
    Timestamp: DateTimeOffset
}

[<Trait("Category", "Standard")>]
module ``LiteDbPersistence Tests`` =
    
    let createPersistence(databaseName: string) =
        
        [
            $"{databaseName}.db"
            $"{databaseName}-log.db"
        ]
        |> List.iter (fun x ->
            if File.Exists x then
                File.Delete x
        )
            
        LiteDbPersistence($"{databaseName}.db")
        
    [<Fact>]
    let ``Find() after inserting an item should return that item`` () =
    
        // Arrange
        let testItem = {
            Id = Guid.Parse "44b8ae0d-37b3-4be3-8992-e7f6832b472a"
            Text = "abc"
            Number = 123
            Timestamp = DateTimeOffset(2023, 12, 25, 15, 30, 0, TimeSpan.FromHours(-5.0))
        }
        
        let persistence = createPersistence("testDatabase")
        
        %persistence.Insert("testCollection", testItem.Id, testItem)
        
        // Act
        let result = persistence.Find("testCollection", testItem.Id)
        
        // Assert
        %result.Should().BeSome()
        %result.Value.Should().Be(testItem)
        persistence.Dispose()
        
    [<Fact>]
    let ``FindAny() with one match returns an array with one item`` () =
    
        // Arrange
        let testItem = {
            Id = Guid.Parse "44b8ae0d-37b3-4be3-8992-e7f6832b472a"
            Text = "abc"
            Number = 123
            Timestamp = DateTimeOffset(2023, 12, 25, 15, 30, 0, TimeSpan.FromHours(-5.0))
        }
        
        let persistence = createPersistence("testDatabase")
        
        %persistence.Insert("testCollection", testItem.Id, testItem)
        
        // Act
        let result = persistence.FindAny<TestModel>("testCollection", "Text = \"abc\"")
        
        // Assert
        %result.Should().HaveLength(1)
        %result[0].Should().Be(testItem)
        persistence.Dispose()
    
    [<Fact>]
    let ``FindAll() for new database should return no items`` () =
    
        // Arrange
        let persistence = createPersistence("testDatabase")
        
        // Act
        let result = persistence.FindAll("testCollection")
        
        // Assert
        %result.Should().BeEmpty()
        persistence.Dispose()
        
    [<Fact>]
    let ``FindAll() after inserting an item should return a single-item collection with that item`` () =
    
        // Arrange
        let testItem = {
            Id = Guid.Parse "44b8ae0d-37b3-4be3-8992-e7f6832b472a"
            Text = "abc"
            Number = 123
            Timestamp = DateTimeOffset(2023, 12, 25, 15, 30, 0, TimeSpan.FromHours(-5.0))
        }
        
        let persistence = createPersistence("testDatabase")
        
        %persistence.Upsert("testCollection", testItem.Id, testItem)
        
        // Act
        let result = persistence.FindAll("testCollection")
        
        // Assert
        %result.Should().HaveLength(1)
        %(result |> Array.head).Should().Be(testItem)
        persistence.Dispose()
