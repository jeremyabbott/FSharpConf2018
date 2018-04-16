module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack
open Fable.PowerPack.Fetch

open Shared

open Fulma
open Fulma.Layouts
open Fulma.Elements
open Fulma.Elements.Form
open Fulma.Components
open Fulma.BulmaClasses
open Shared.Pokemon
open System.ComponentModel


// live demo todo
// Add selected
// Add clicked event
// Add messages
// Add action
//
type Model = { Loading: bool;
               ErrorText: string option;
               Results: PokemonList;
               SearchText: string;
               SelectedPokemon: Pokemon option }

type Msg =
| Init of PokemonListResponse
| LoadList of PokemonList
| LoadError of exn
| PokemonClicked of string
| PokemonLoading
| PokemonLoaded of Pokemon


let loadInitialList () = promise {
  let requestProperties =
      [ RequestProperties.Method HttpMethod.GET
        Fetch.requestHeaders [ HttpRequestHeaders.ContentType "application/json" ]]
  let url = sprintf "/api/pokemon"
  try
      let! response = Fetch.fetchAs<PokemonListResponse> url requestProperties
      match response with
      | Ok r -> return r
      | Error ex -> return! failwithf "Something bad happened %s" ex.Message
  with ex -> return! failwithf "Something bad happened %s" ex.Message
}

let initCmd () =
  Cmd.ofPromise loadInitialList () LoadList LoadError

let init () : Model * Cmd<Msg> =
  let model = { Loading=true
                ErrorText = None
                Results = [||]
                SearchText = ""
                SelectedPokemon = None }

  model, (initCmd())

let loadPokemon s = promise {
  let requestProperties =
      [ RequestProperties.Method HttpMethod.GET
        Fetch.requestHeaders [ HttpRequestHeaders.ContentType "application/json" ]]
  let url = sprintf "/api/pokemon/%s" s
  try
      let! response = Fetch.fetchAs<PokemonResonse> url requestProperties
      match response with
      | Ok r -> return r
      | Error ex -> return! failwithf "Something bad happened %s" ex.Message
  with ex -> return! failwithf "Something bad happened %s" ex.Message
}

let loadPokemonCmd s =
  Cmd.ofPromise loadPokemon s PokemonLoaded LoadError

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
  let model', cmd =
    match msg with
    | Init plr ->
      match plr with
      | Ok pl -> model, Cmd.ofMsg (LoadList pl)
      | Error _ -> model, Cmd.none
    | LoadError e -> { model with ErrorText = Some e.Message; Loading = false }, Cmd.none
    | LoadList pl -> { model with Results = pl; Loading=false }, Cmd.none
    | PokemonClicked s -> { model with SearchText = s; Loading = true}, loadPokemonCmd s
    | PokemonLoaded p -> { model with SelectedPokemon = Some p; Loading = false}, Cmd.none
    | _ -> model, Cmd.none
  model', cmd

let safeComponents =
  let intersperse sep ls =
    List.foldBack (fun x -> function
      | [] -> [x]
      | xs -> x::sep::xs) ls []

  let components =
    [
      "Saturn", "https://saturnframework.github.io/docs/"
      "Fable", "http://fable.io"
      "Elmish", "https://fable-elmish.github.io/"
      "Fulma", "https://mangelmaxime.github.io/Fulma"
    ]
    |> List.map (fun (desc,link) -> a [ Href link ] [ str desc ] )
    |> intersperse (str ", ")
    |> span [ ]

  p [ ]
    [ strong [] [ str "SAFE Template" ]
      str " powered by: "
      components ]

let resultsColumns model dispatch =
    model.Results
    |> Array.map (fun p ->
              Column.column
                [
                 Column.Width (Column.All, Column.IsNarrow)
                 Column.Width (Column.All, Column.IsHalf)
                 Column.CustomClass "has-text-centered"]
                [
                  Button.button
                    [ Button.Color IsInfo
                      Button.IsInverted
                      Button.OnClick (fun ev -> ev.preventDefault(); p.Name |> PokemonClicked |> dispatch) ]
                    [ str p.Name ] ])
    |> Array.toList

let selectedPokemonColumn model dispatch =
  Column.column [] [
    match model with
    | { Loading = true} ->
      yield str "Loading ..."
    | { SelectedPokemon = Some pokemon} ->
        yield
          Card.card [ ] [
            Card.content [ ] [
              Media.media [] [
                Media.left [] [
                  Image.image [ Image.Is128x128 ] [img [ Src pokemon.ImageUrl ]]
                ]
                Media.content [] [
                  p [ClassName "title is-4"] [pokemon.Number |> sprintf "Pokedex Number: %i" |> str]
                  p [ClassName "subtitle is-6"] [pokemon.Name |> sprintf "Name: %s" |> str]
                ]
              ]
            ]
          ]
    | { ErrorText = Some m } ->
      yield str m
    | { ErrorText = None; SelectedPokemon = None } -> yield str "Click on a Pokemon!"
  ]

let pokemonContent model dispatch =
  Columns.columns [] [
    // Results Column
    Column.column [Column.Width(Column.All, Column.IsHalf)] [
      Columns.columns [Columns.IsCentered; Columns.IsMultiline] (resultsColumns model dispatch)
    ]
    // Clicked Pokemon
    selectedPokemonColumn model dispatch
  ]

let view (model : Model) (dispatch : Msg -> unit) =

  div []
    [ Navbar.navbar [ Navbar.Color IsPrimary ]
        [ Navbar.Item.div [ ]
            [ Heading.h2 [ ]
                [ str "SAFE Template" ] ] ]

      Container.container []
        [ Content.content [ Content.CustomClass Bulma.Properties.Alignment.HasTextCentered ]
            [ Heading.h3 [] [ str ("Pokemon and F#") ]
              Heading.h4 [] [ str "Gotta Catch 'Em All SAFEly!"]]
          pokemonContent model dispatch
          // str "Content goes here!"
        ]

      Footer.footer [ ]
        [ Content.content [ Content.CustomClass Bulma.Properties.Alignment.HasTextCentered ]
            [ safeComponents ] ] ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
