﻿// IMPORTANT:  READ BEFORE DOWNLOADING, COPYING, INSTALLING OR USING.
// By downloading, copying, installing or using the software you agree
// to this license.  If you do not agree to this license, do not
// download, install, copy or use the software.
// 
// Copyright (c) 2003-2007, John Harrison
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 
// * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// 
// * The name of John Harrison may not be used to endorse or promote
// products derived from this software without specific prior written
// permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF
// USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
// SUCH DAMAGE.
//
// ===================================================================
//
// Converted to F# 2.0
//
// Copyright (c) 2012, Eric Taucher
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 
// * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the previous disclaimer.
// 
// * Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the previous disclaimer in the
// documentation and/or other materials provided with the distribution.
// 
// * The name of Eric Taucher may not be used to endorse or promote
// products derived from this software without specific prior written
// permission.
//
// ===================================================================

namespace Reasoning.Automated.Harrison.Handbook

module herbrand =

    open lib
    open intro
    open formulas
    open prop
//    open propexamples
//    open defcnf 
    open dp
//    open stal
//    open bdd
    open folMod
    open skolem

// ========================================================================= //
// Relation between FOL and propositonal logic; Herbrand theorem.            //
//                                                                           //
// Copyright (c) 2003-2007, John Harrison. (See "LICENSE.txt" for details.)  //
// ========================================================================= //
//
// pg. 151
// ------------------------------------------------------------------------- //
// Propositional valuation.                                                  //
// ------------------------------------------------------------------------- //

    let pholds d fm = eval fm (fun p -> d(Atom p))

// pg. 156
// ------------------------------------------------------------------------- //
// Get the constants for Herbrand base, adding nullary one if necessary.     //
// ------------------------------------------------------------------------- //

    let herbfuns fm =
        let cns,fns = List.partition (fun (_,ar) -> ar = 0) (functions fm)
        if cns = [] then ["c",0],fns else cns,fns

// pg. 159
// ------------------------------------------------------------------------- //
// Enumeration of ground terms and m-tuples, ordered by total fns.           //
// ------------------------------------------------------------------------- //

    let rec groundterms cntms funcs n =
        if n = 0 then cntms else
        itlist (fun (f,m) l -> 
            List.map (fun args -> 
                Fn(f,args))
                (groundtuples cntms funcs (n - 1) m) @ l)
            funcs []

    and groundtuples cntms funcs n m =
        if m = 0 then 
            if n = 0 then [[]] 
            else [] 
        else
            itlist (fun k l -> 
                allpairs (fun h t -> h::t)
                    (groundterms cntms funcs k)
                    (groundtuples cntms funcs (n - k) (m - 1)) @ l)
                    (0 -- n) []

// pg. 160
// ------------------------------------------------------------------------- //
// Iterate modifier "mfn" over ground terms till "tfn" fails.                //
// ------------------------------------------------------------------------- //

    let rec herbloop mfn tfn fl0 cntms funcs fvs n fl tried tuples =
        printfn "%s" (string(List.length tried) + " ground instances tried; " + 
                    string(List.length fl) + " items in list");
        match tuples with
        | [] -> let newtups = groundtuples cntms funcs n (List.length fvs)
                herbloop mfn tfn fl0 cntms funcs fvs (n + 1) fl tried newtups
        | tup::tups ->
                let fl' = mfn fl0 (subst(fpf fvs tup)) fl
                if not(tfn fl') then tup::tried 
                else herbloop mfn tfn fl0 cntms funcs fvs n fl' (tup::tried) tups

// pg. 160
// ------------------------------------------------------------------------- //
// Hence a simple Gilmore-type procedure.                                    //
// ------------------------------------------------------------------------- //

    let gilmore_loop =
        let mfn djs0 ifn djs =
            List.filter (non trivial) (distrib (image (image ifn) djs0) djs)
        herbloop mfn (fun djs -> djs <> [])

    let gilmore fm =
        let sfm = skolemize(Not(generalize fm))
        let fvs = fv sfm 
        let consts,funcs = herbfuns sfm
        let cntms = image (fun (c,_) -> Fn(c,[])) consts
        List.length(gilmore_loop (simpdnf sfm) cntms funcs fvs 0 [[]] [] [])

// pg. 163
// ------------------------------------------------------------------------- //
// The Davis-Putnam procedure for first order logic.                         //
// ------------------------------------------------------------------------- //

    let dp_mfn cjs0 ifn cjs = union (image (image ifn) cjs0) cjs

    let dp_loop = herbloop dp_mfn dpll

    let davisputnam fm =
        let sfm = skolemize(Not(generalize fm))
        let fvs = fv sfm 
        let consts,funcs = herbfuns sfm
        let cntms = image (fun (c,_) -> Fn(c,[])) consts
        List.length(dp_loop (simpcnf sfm) cntms funcs fvs 0 [] [] [])

// pg. 163
// ------------------------------------------------------------------------- //
// Try to cut out useless instantiations in final result.                    //
// ------------------------------------------------------------------------- //

    let rec dp_refine cjs0 fvs dunno need =
        match dunno with
        | [] -> need
        | cl::dknow ->
            let mfn = dp_mfn cjs0 ** subst ** fpf fvs
            let need' =
                if dpll(itlist mfn (need @ dknow) []) then cl::need 
                else need
            dp_refine cjs0 fvs dknow need'

    let dp_refine_loop cjs0 cntms funcs fvs n cjs tried tuples =
        let tups = dp_loop cjs0 cntms funcs fvs n cjs tried tuples
        dp_refine cjs0 fvs tups []

// pg. 163
// ------------------------------------------------------------------------- //
// Show how few of the instances we really need. Hence unification!          //
// ------------------------------------------------------------------------- //

    let davisputnam' fm =
        let sfm = skolemize(Not(generalize fm))
        let fvs = fv sfm 
        let consts,funcs = herbfuns sfm
        let cntms = image (fun (c,_) -> Fn(c,[])) consts
        List.length(dp_refine_loop (simpcnf sfm) cntms funcs fvs 0 [] [] [])
