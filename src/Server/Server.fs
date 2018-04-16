open System.IO
open System.Threading.Tasks

open Giraffe
open Saturn

open Giraffe.Serialization
open Microsoft.Extensions.DependencyInjection

open Shared

let clientPath = Path.Combine("..","Client") |> Path.GetFullPath
let port = 8085us


let browserRouter = scope {
  get "/" (htmlFile (Path.Combine(clientPath, "index.html")))
}

let config (services:IServiceCollection) =
  let fableJsonSettings = Newtonsoft.Json.JsonSerializerSettings()
  fableJsonSettings.Converters.Add(Fable.JsonConverter())
  services.AddSingleton<IJsonSerializer>(NewtonsoftJsonSerializer fableJsonSettings) |> ignore
  services

let apiRouter = scope {
  forward "/pokemon" Pokemon.Controller.controller
}

let mainRouter = scope {
  forward "" browserRouter
  forward "/api" apiRouter
}

let app = application {
    router mainRouter
    url ("http://0.0.0.0:" + port.ToString() + "/")
    memory_cache
    use_static clientPath
    service_config config
    use_gzip
}

run app