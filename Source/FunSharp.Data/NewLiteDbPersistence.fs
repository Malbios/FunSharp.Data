namespace FunSharp.Data

open FunSharp.Common
open FunSharp.Data.Abstraction
open LiteDB

type NewLiteDbPersistence(databaseFilePath: string) =
    
    let db = new LiteDatabase(databaseFilePath)
    
    let withCollection (collectionName: string) f =
        
        let collection = db.GetCollection<BsonDocument>(collectionName)
        f collection
        
    member private _.AsBson<'T>(value: 'T) =
        let doc = BsonDocument()
        doc["data"] <- JsonSerializer.serialize value
        doc
        
    member private _.AsValue<'T>(doc: BsonDocument) =
        doc["data"].AsString |> JsonSerializer.deserialize<'T>
        
    member private _.AsBsonKey<'Key>(key: 'Key) =
        BsonValue(key.ToString())
        
    interface IPersistence with
        
        member _.Dispose() =
            
            db.Dispose()
            
        member this.Insert<'Key, 'Value when 'Value : not struct and 'Value : equality and 'Value : not null>
            (collectionName, key: 'Key, value: 'Value) =
                
                withCollection collectionName _.Insert(this.AsBsonKey key, this.AsBson value)
                
        member this.Update<'Key, 'Value when 'Value : not struct and 'Value : equality and 'Value : not null>
            (collectionName, key: 'Key, value: 'Value) =
                
                withCollection collectionName _.Update(this.AsBsonKey key, this.AsBson value)
                
        member this.Upsert<'Key, 'Value when 'Value : not struct and 'Value : equality and 'Value : not null>
            (collectionName, key: 'Key, value: 'Value) =
                
                match withCollection collectionName _.Upsert(this.AsBsonKey key, this.AsBson value) with
                | true -> UpsertResult.Insert
                | false -> UpsertResult.Update
                
        member this.Find<'Key, 'Value when 'Value : not struct and 'Value : equality and 'Value : not null>
            (collectionName, key: 'Key) : 'Value option =
                
                withCollection collectionName (fun collection -> collection.FindById(this.AsBsonKey key) |> Option.ofObj |> Option.map this.AsValue)
                
        member this.FindAll<'Value when 'Value : not struct and 'Value : equality and 'Value : not null>
            collectionName : 'Value array =
                
                withCollection collectionName (fun collection -> collection.FindAll() |> Seq.toArray |> Array.map this.AsValue)
                
        member this.FindAny<'Value when 'Value : not struct and 'Value : equality and 'Value : not null>
            (collectionName, query) : 'Value array =
                
                let this = this :> IPersistence
                this.FindAll(collectionName) |> Array.filter query
                
        member this.Delete<'Key>(collectionName, key: 'Key) =
            
            withCollection collectionName _.Delete(this.AsBsonKey key)
