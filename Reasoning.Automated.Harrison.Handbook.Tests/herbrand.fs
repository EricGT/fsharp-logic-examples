﻿// ========================================================================= //
// Copyright (c) 2003-2007, John Harrison.                                   //
// Copyright (c) 2012 Eric Taucher, Jack Pappas, Anh-Dung Phan               //
// (See "LICENSE.txt" for details.)                                          //
// ========================================================================= //

namespace Reasoning.Automated.Harrison.Handbook.Tests

module herbrand =
    open NUnit.Framework
    open FsUnit

    open Reasoning.Automated.Harrison.Handbook.lib    
    open Reasoning.Automated.Harrison.Handbook.formulas
    open Reasoning.Automated.Harrison.Handbook.prop
    open Reasoning.Automated.Harrison.Handbook.dp
    open Reasoning.Automated.Harrison.Handbook.folMod
    open Reasoning.Automated.Harrison.Handbook.skolem
    open Reasoning.Automated.Harrison.Handbook.herbrand

    // pg. 161
    // ------------------------------------------------------------------------- //
    // First example and a little tracing.                                       //
    // ------------------------------------------------------------------------- //
    
    [<Test>]
    let ``test gilmore simple``() =
        gilmore (parse "exists x. forall y. P(x) ==> P(y)")
        |> should equal 2

    // pg. 161
    // ------------------------------------------------------------------------- //
    // Quick example.                                                            //
    // ------------------------------------------------------------------------- //

    [<Test>]
    let ``test gilmore quick``() =
        gilmore (parse "~(exists x. U(x) /\ Q(x)) 
            /\ (forall x. P(x) ==> Q(x) \/ R(x)) 
            /\ ~(exists x. P(x) ==> (exists x. Q(x))) 
            /\ (forall x. Q(x) 
            /\ R(x) ==> U(x)) ==> (exists x. P(x) /\ R(x))")
        |> should equal 1

    // pg. 162
    // ------------------------------------------------------------------------- //
    // Slightly less easy example.                                               //
    // ------------------------------------------------------------------------- //

    // TODO: fix this stackoverflow error

//    let p45 = gilmore (parse "(forall x. P(x) 
//    /\ (forall y. G(y) /\ H(x,y) ==> J(x,y)) ==> (forall y. G(y) /\ H(x,y) ==> R(y))) 
//    /\ ~(exists y. L(y) /\ R(y)) 
//    /\ (exists x. P(x) /\ (forall y. H(x,y) ==> L(y)) 
//    /\ (forall y. G(y) /\ H(x,y) ==> J(x,y))) ==> (exists x. P(x) /\ ~(exists y. G(y) /\ H(x,y)))")

    // pg. 162
    // ------------------------------------------------------------------------- //
    // Apparently intractable example.                                           //
    // ------------------------------------------------------------------------- //

    // TODO: fix this stackoverflow error

//    let p20 = gilmore (parse "(forall x y. exists z. forall w. P(x) /\ Q(y) ==> R(z) /\ U(w))
//       ==> (exists x y. P(x) /\ Q(y)) ==> (exists z. R(z))")
