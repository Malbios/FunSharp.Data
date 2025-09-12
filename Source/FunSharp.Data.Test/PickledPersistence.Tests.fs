namespace FunSharp.Data.Test

open System
open System.IO
open Xunit
open Faqt
open Faqt.Operators
open FunSharp.Data
open FunSharp.Data.Abstraction

type TestModelA = {
    Id: Guid
    Text: string
    Number: int
}

type TestModelB = {
    Id: Guid
    Text: string
    Timestamp: DateTimeOffset
}

type MyDU =
    | A of TestModelA
    | B of TestModelB

[<Trait("Category", "Standard")>]
module ``PickledPersistence Tests`` =
    
    let createPersistence(databaseName: string) =
        
        [
            $"{databaseName}.db"
            $"{databaseName}-log.db"
        ]
        |> List.iter (fun x ->
            if File.Exists x then
                File.Delete x
        )
            
        new PickledPersistence($"{databaseName}.db") :> IPersistence
        
    [<Fact>]
    let ``Find() after inserting an item should return that item`` () =
    
        // Arrange
        let id = Guid.Parse "44b8ae0d-37b3-4be3-8992-e7f6832b472a"
        
        let testItem = MyDU.A {
            Id = id
            Text = "abc"
            Number = 123
        }
        
        let persistence = createPersistence("testDatabase")
        
        %persistence.Insert("testCollection", id, testItem)
        
        // Act
        let result = persistence.Find("testCollection", id)
        
        // Assert
        %result.Should().BeSome()
        %result.Value.Should().Be(testItem)
        persistence.Dispose()
        
    [<Fact>]
    let ``FindAny() with one match returns an array with one item`` () =
    
        // Arrange
        let testItem : TestModelA = {
            Id = Guid.Parse "44b8ae0d-37b3-4be3-8992-e7f6832b472a"
            Text = "abc"
            Number = 123
        }
        
        let persistence = createPersistence("testDatabase")
        
        %persistence.Insert("testCollection", testItem.Id, testItem)
        
        // Act
        let result = persistence.FindAny<TestModelA>("testCollection", (fun x -> x.Text = "abc"))
        
        // Assert
        %result.Should().HaveLength(1)
        %result[0].Should().Be(testItem)
        persistence.Dispose()
    
    [<Fact>]
    let ``FindAll() for new database should return no items`` () =
    
        // Arrange
        let persistence = createPersistence("testDatabase")
        
        // Act
        let result = persistence.FindAll<MyDU>("testCollection")
        
        // Assert
        %result.Should().BeEmpty()
        persistence.Dispose()
        
    [<Fact>]
    let ``FindAll() after inserting an item should return a single-item collection with that item`` () =
    
        // Arrange
        let id = Guid.Parse "44b8ae0d-37b3-4be3-8992-e7f6832b472a"
        
        let testItem = MyDU.B {
            Id = id
            Text = "abc"
            Timestamp = DateTimeOffset(2023, 12, 25, 15, 30, 0, TimeSpan.FromHours(-5.0))
        }
        
        let persistence = createPersistence("testDatabase")
        
        %persistence.Upsert("testCollection", id, testItem)
        
        // Act
        let result = persistence.FindAll("testCollection")
        
        // Assert
        %result.Should().HaveLength(1)
        %(result |> Array.head).Should().Be(testItem)
        persistence.Dispose()
