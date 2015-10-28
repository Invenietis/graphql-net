module public GraphQLParser

open FParsec
open FParsec.CharParsers

type Alias = string option

type Input = Int of int
           | Float of single
           | String of string
           | Boolean of bool
type Argument = string * Input
type Arguments = Argument list

type Directive = string * Arguments option
type Directives = Directive list

type Selection = Field of alias : Alias  * fragmentIdentifier : string option * name : string * arguments : Arguments * directives : Directives * selectionSet : SelectionSet
and SelectionSet = Selection list

type Fragment =  Fragment of string * string * SelectionSet
type Fragments = Fragment list
type Query = string * Arguments * SelectionSet * Fragments

type Definition = QueryOperation of Query
                | MutationOperation
                | TypeExt
                | TypeDef
                | Enum
                | Definition

type Document = Definition List

let str s = pstring s
let ws = many (skipChar ',' <|> spaces1)

let name =
    let isNameFirstChar c = isLetter c || c = '_'
    let isNameChar c = isLetter c || isDigit c || c = '_'
    many1Satisfy2L isNameFirstChar isNameChar "name" .>> ws

let pnum = numberLiteral (NumberLiteralOptions.AllowExponent ||| NumberLiteralOptions.AllowMinusSign) "number" .>> ws
        |>> fun n ->
            if (n.IsInteger) then Int(int32 n.String)
            else Float(single n.String)
let pbool = str "true" .>> ws |>> (fun a -> Boolean(true)) <|> (str "false" .>> ws |>> (fun a -> Boolean(false)))
// TODO pstr
// TODO pguid
let value = pbool <|> pnum

let alias = name .>> str ":" .>> ws
let argument = name .>>. (ws >>. str ":" >>. ws >>. value .>> ws)
let arguments = str "(" >>. ws >>. many argument .>> str ")" .>> ws
let fragIdentifier = str "..."

let directive = str "@" >>. name .>>. opt (attempt arguments)
let directives = many1 directive

let field, fieldref = createParserForwardedToRef<Selection, unit>()
let selectionset = between (str "{") (str "}") (ws >>. many field) .>> ws

let coalesce optList =
    match optList with
    | Some opts -> opts
    | None -> List.Empty

let pipe6 p1 p2 p3 p4 p5 p6 f = 
    pipe5 p1 p2 p3 p4 (tuple2 p5 p6)
          (fun x1 x2 x3 x4 (x5, x6) -> f x1 x2 x3 x4 x5 x6) 

do fieldref := pipe6 
    (opt (attempt alias))
    (opt (attempt fragIdentifier))
    name
    (opt (attempt arguments))
    (opt (attempt directives))
    (opt (attempt selectionset))
    (fun alias fragmentIdentifier name args dirs set-> Field(alias, fragmentIdentifier, name, coalesce args, coalesce dirs, coalesce set ))

let fragment = pipe3
                (ws >>. str "fragment" >>. ws >>. name )
                (ws >>. str "on" >>. ws >>. name )
                selectionset
                (fun q objectEntity s -> Fragment( q, objectEntity, s ))
let fragments = ws >>. many1 fragment .>> ws

let query = pipe4
                (ws >>. str "query" >>. ws >>. name )
                (opt (attempt arguments))
                selectionset
                (opt (attempt fragments))
                (fun q args s f-> QueryOperation( q, coalesce args, s, coalesce f ))


let parse str = 
        match run query str with
        | Success(result, _, _) -> Some result
        | Failure(errorMsg, _, _) -> None