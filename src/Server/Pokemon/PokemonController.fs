namespace Pokemon

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.ContextInsensitive
open Saturn
open Shared.Pokemon

module Controller =
    open ApiWrapper

    let indexAction (ctx: HttpContext) = task {
        let! result = getPokemonList ""
        match result with
        | Ok _ ->
            return! Controller.json ctx result
        | Error ex ->
            return raise ex
    }

    let showAction (ctx: HttpContext, name : string) = task {
        let! result = getPokemonById (name.ToLower())
        match result with
        | Ok _ ->
            return! Controller.json ctx result
        | Error ex ->
            return raise ex
    }

    let controller = controller {
        index indexAction
        show showAction
    }