﻿namespace Funtom.Linq

open System.Collections
open System.Collections.Generic
open System
open System.Diagnostics

module Core =
  let inline combine_predicates ([<InlineIfLambda>] p1: 'source -> bool) ([<InlineIfLambda>] p2: 'source -> bool) (x: 'source) = p1 x && p2 x

  /// <summary>
  /// 
  /// </summary>
  type WhereEnumerableIterator<'source> (source: seq<'source>, [<InlineIfLambda>] predicate: 'source -> bool) as self =
    let mutable state : int = 0
    let mutable enumerator : IEnumerator<'source> = Unchecked.defaultof<IEnumerator<'source>>
    let mutable current : 'source = Unchecked.defaultof<'source>
    let thread_id : int = Environment.CurrentManagedThreadId
    
    let clone () = 
      new WhereEnumerableIterator<'source>(source, predicate)

    let dispose () = 
      if enumerator <> Unchecked.defaultof<IEnumerator<'source>> then
        enumerator.Dispose()
        enumerator <- Unchecked.defaultof<IEnumerator<'source>>
        current <- Unchecked.defaultof<'source>
        state <- -1

    let get_enumerator () =
      if state = 0 && thread_id = Environment.CurrentManagedThreadId then
        state <- 1
        self
      else
        let enumerator = clone()
        enumerator.State <- 1
        enumerator
      
    let rec move_next () =
      if enumerator.MoveNext() then
        let item = enumerator.Current
        if predicate item then
          current <- item
          true
        else
          move_next ()
      else
        dispose()
        false

    interface IDisposable with
      member __.Dispose () = dispose ()

    interface IEnumerator with
      member __.MoveNext () : bool = 

        match state with
        | 1 -> 
          enumerator <- source.GetEnumerator()
          state <- 2
          move_next ()
        | 2 ->
          move_next ()
        | _ -> 
          false

      member __.Current with get() = current
      member __.Reset () = raise(NotSupportedException "not supported")

    interface IEnumerator<'source> with
      member __.Current with get() = current

    interface IEnumerable with
      member __.GetEnumerator () = get_enumerator ()

    interface IEnumerable<'source> with
      member __.GetEnumerator () = get_enumerator ()

    member __.where (predicate': 'source -> bool) =
      let p = combine_predicates predicate predicate'
      new WhereEnumerableIterator<'source>(source, p)

    member private __.State with get() = state and set v = state <- v

  /// <summary>
  /// 
  /// </summary>
  type WhereArrayIterator<'source> (source: array<'source>, [<InlineIfLambda>] predicate: 'source -> bool) as self =
    let mutable state : int = 0
    let mutable current : 'source = Unchecked.defaultof<'source>
    let thread_id : int = Environment.CurrentManagedThreadId
  
    let clone () = new WhereArrayIterator<'source>(source, predicate)
  
    let dispose () = 
      current <- Unchecked.defaultof<'source>
      state <- -1
  
    let get_enumerator () =
      if state = 0 && thread_id = Environment.CurrentManagedThreadId then
        state <- 1
        self
      else
        let enumerator = clone()
        enumerator.State <- 1
        enumerator
  
    let rec move_next (src: array<'source>, index: int) =
      if uint index < uint src.Length then
        let item = src[index]
        state <- state + 1
        if predicate item then
          current <- item
          true
        else
          move_next(src, index + 1)
      else
        dispose()
        false
  
    interface IDisposable with
      member __.Dispose () = dispose ()
  
    interface IEnumerator with
      member __.MoveNext () : bool = move_next (source, state - 1)
      member __.Current with get() = current
      member __.Reset () = raise(NotSupportedException "not supported")
  
    interface IEnumerator<'source> with
      member __.Current with get() = current
  
    interface IEnumerable with
      member __.GetEnumerator () = get_enumerator ()
  
    interface IEnumerable<'source> with
      member __.GetEnumerator () = get_enumerator ()
  
    member __.where (predicate': 'source -> bool) =
      let p = combine_predicates predicate predicate'
      new WhereArrayIterator<'source>(source, p)
  
    member private __.State with get() = state and set v = state <- v

  /// <summary>
  /// 
  /// </summary>
  type WhereListIterator<'source> (source: ResizeArray<'source>, [<InlineIfLambda>] predicate: 'source -> bool) as self =
    let mutable state : int = 0
    let mutable current : 'source = Unchecked.defaultof<'source>
    let thread_id : int = Environment.CurrentManagedThreadId
  
    let clone () = new WhereListIterator<'source>(source, predicate)
  
    let dispose () = 
      current <- Unchecked.defaultof<'source>
      state <- -1
  
    let get_enumerator () =
      if state = 0 && thread_id = Environment.CurrentManagedThreadId then
        state <- 1
        self
      else
        let enumerator = clone()
        enumerator.State <- 1
        enumerator
  
    let rec move_next (src: ResizeArray<'source>, index: int) =
      if uint index < uint src.Count then
        let item = src[index]
        state <- state + 1
        if predicate item then
          current <- item
          true
        else
          move_next(src, index + 1)
      else
        dispose()
        false
  
    interface IDisposable with
      member __.Dispose () = dispose ()
  
    interface IEnumerator with
      member __.MoveNext () : bool = move_next (source, state - 1)  
      member __.Current with get() = current
      member __.Reset () = raise(NotSupportedException "not supported")
  
    interface IEnumerator<'source> with
      member __.Current with get() = current
  
    interface IEnumerable with
      member __.GetEnumerator () = get_enumerator ()
  
    interface IEnumerable<'source> with
      member __.GetEnumerator () = get_enumerator ()
  
    member __.where (predicate': 'source -> bool) =
      let p = combine_predicates predicate predicate'
      new WhereListIterator<'source>(source, p)
  
    member private __.State with get() = state and set v = state <- v
  
  /// <summary>
  /// 
  /// </summary>
  type WhereFsListIterator<'source> (source: list<'source>, [<InlineIfLambda>] predicate: 'source -> bool) as self =
    let mutable state : int = 0
    let mutable cache : list<'source> = source
    let mutable current : 'source = Unchecked.defaultof<'source>
    let thread_id : int = Environment.CurrentManagedThreadId
  
    let clone () = new WhereFsListIterator<'source>(source, predicate)
  
    let dispose () = 
      current <- Unchecked.defaultof<'source>
      state <- -1
  
    let get_enumerator () =
      if state = 0 && thread_id = Environment.CurrentManagedThreadId then
        state <- 1
        self
      else
        let enumerator = clone()
        enumerator.State <- 1
        enumerator
  
    let rec move_next (src: list<'source>) =
      match src with
      | h::tail ->
        if predicate h then
          current <- h
          cache <- tail
          true
        else
          move_next(tail)
      | _ ->
        dispose()
        false
  
    interface IDisposable with
      member __.Dispose () = dispose ()
  
    interface IEnumerator with
      member __.MoveNext () : bool = 
        move_next cache
  
      member __.Current with get() = current
      member __.Reset () = raise(NotSupportedException "not supported")
  
    interface IEnumerator<'source> with
      member __.Current with get() = current
  
    interface IEnumerable with
      member __.GetEnumerator () = get_enumerator ()
  
    interface IEnumerable<'source> with
      member __.GetEnumerator () = get_enumerator ()
  
    member __.where (predicate': 'source -> bool) =
      let p = combine_predicates predicate predicate'
      new WhereFsListIterator<'source>(source, p)
  
    member private __.State with get() = state and set v = state <- v