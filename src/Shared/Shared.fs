namespace Shared

module Pokemon =
    type PokemonListItem = {
        Name: string
    }

    type PokemonList = PokemonListItem array

    type PokemonListResponse = Result<PokemonList,exn>

    type Pokemon = {
        Id: int
        Name: string
        Number: int
        ImageUrl: string
    }

    type PokemonResonse = Result<Pokemon,exn>