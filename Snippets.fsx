// 1. Create Controller

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

    let showAction (ctx: HttpContext, id : string) = task {
        let! result = getPokemonById id
        match result with
        | Ok result ->
            return! Controller.json ctx result
        | Error ex ->
            return raise ex
    }

    let controller = controller {
        index indexAction
        show showAction
    }

// 2 Create apiRouter

let apiRouter = scope {
  forward "/pokemon" Pokemon.Controller.controller
}

// 3 For requests to controller in mainRouter
let mainRouter = scope {
  forward "" browserRouter
  forward "/api" apiRouter
}

// 4 Create Model
type Model = { Results: PokemonList }

type Msg =
| Init of PokemonListResponse
| LoadList of PokemonList
| LoadError of exn

// 5 Update init functions
let initCmd () =
  Cmd.ofPromise loadInitialList () LoadList LoadError

let init () : Model * Cmd<Msg> =
  let model = { Results = [||] }

  model, (initCmd())

// 6 Update update function


let update (msg : Msg) (model : Model) : Model * Cmd<Msg> =
  let model', cmd =
    match msg with
    | Init plr ->
      match plr with
      | Ok pl -> model, Cmd.ofMsg (LoadList pl)
      | Error _ -> model, Cmd.none
    | LoadList pl -> { model with Results = pl }, Cmd.none
    | _ -> model, Cmd.none
  model', cmd