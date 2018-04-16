namespace Pokemon
open FSharp.Control.Tasks.ContextInsensitive
open FSharp.Data
open Shared.Pokemon

module ApiWrapper =

    type PokemonList = JsonProvider<"http://pokeapi.co/api/v2/pokemon">
    type Pokemon = JsonProvider<"http://pokeapi.co/api/v2/pokemon/1">

    let toTitleCase s =
      let ti = (System.Globalization.CultureInfo("en-US", false).TextInfo)
      ti.ToTitleCase s

    let getPokemonList pageNumber = task {
        let url = "http://pokeapi.co/api/v2/pokemon"
        try
            let! pListResponse = PokemonList.AsyncLoad(url)
            let pList =
                pListResponse.Results
                |> Array.map (fun p -> { Name = (toTitleCase p.Name)})
            return Ok pList
        with ex -> return Error ex
    }

    let getPokemonById name = task {
        let url = sprintf "http://pokeapi.co/api/v2/pokemon/%s" name
        try
            let! pokemonResponse = Pokemon.AsyncLoad(url)
            return { Id = pokemonResponse.Id
                     Name = (toTitleCase pokemonResponse.Name)
                     Number = pokemonResponse.Id
                     ImageUrl = pokemonResponse.Sprites.FrontDefault }
                   |> Ok
        with ex -> return Error ex
    }