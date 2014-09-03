'(C) Copyright 2012 by Autodesk, Inc. 

'Permission to use, copy, modify, and distribute this software
'in object code form for any purpose and without fee is hereby
'granted, provided that the above copyright notice appears in
'all copies and that both that copyright notice and the limited
'warranty and restricted rights notice below appear in all
'supporting documentation.

'AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
'AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
'MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK,
'INC. DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL
'BE UNINTERRUPTED OR ERROR FREE.

'Use, duplication, or disclosure by the U.S. Government is
'subject to restrictions set forth in FAR 52.227-19 (Commercial
'Computer Software - Restricted Rights) and DFAR 252.227-7013(c)
'(1)(ii)(Rights in Technical Data and Computer Software), as
'applicable.

' Written by Adam Nagy

Imports System
Imports Autodesk.AutoCAD.Runtime
Imports Autodesk.AutoCAD.ApplicationServices
Imports Autodesk.AutoCAD.DatabaseServices
Imports Autodesk.AutoCAD.Geometry
Imports Autodesk.AutoCAD.EditorInput
Imports acApp = Autodesk.AutoCAD.ApplicationServices.Application

' This line is not mandatory, but improves loading performances
<Assembly: CommandClass(GetType(AcmUtility.MyCommands))> 
Namespace AcmUtility

  ' This class is instantiated by AutoCAD for each document when
  ' a command is called by the user the first time in the context
  ' of a given document. In other words, non static data in this class
  ' is implicitly per-document!
  Public Class MyCommands

    ' Application Session Command with localized name
    <CommandMethod("AcmUtilityConvertXlines")> _
    Public Shared Sub AcmUtilConvertXlines() ' This method can have any name
      Dim doc As Document = acApp.DocumentManager.MdiActiveDocument

      Dim db As Database = doc.Database

      Using tr As Transaction = db.TransactionManager.StartTransaction()

        ' Just something crude to start with
        Dim length As Double
        length = db.Extmin.DistanceTo(db.Extmax) * 2

        Dim bt As BlockTable
        bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead)

        For Each blockId In bt
          Dim block As BlockTableRecord
          block = tr.GetObject(blockId, OpenMode.ForRead)

          Dim modified As Boolean = False

          For Each entId In block
            If entId.ObjectClass = Xline.GetClass(GetType(Xline)) Then
              If Not modified Then
                block.UpgradeOpen()
                modified = True
              End If

              Dim xl As Xline = tr.GetObject(entId, OpenMode.ForWrite)

              Using l As New Line
                l.StartPoint = xl.BasePoint.Add(xl.UnitDir.MultiplyBy(-length))
                l.EndPoint = xl.BasePoint.Add(xl.UnitDir.MultiplyBy(length))
                l.Layer = xl.Layer
                l.Color = xl.Color
                block.AppendEntity(l)
                tr.AddNewlyCreatedDBObject(l, True)
              End Using

              xl.Erase()
            End If
          Next

          ' If the block got modified then let's update its references
          If modified Then
            For Each brId As ObjectId In block.GetBlockReferenceIds(True, True)
              Dim br As BlockReference
              br = tr.GetObject(brId, OpenMode.ForWrite)
              br.RecordGraphicsModified(True)
            Next
          End If
        Next

        tr.Commit()
      End Using
    End Sub

    Shared ltscale As Double

    ' Application Session Command with localized name
    <CommandMethod("AcmUtilityHatch")> _
    Public Shared Sub AcmUtilHatch() ' This method can have any name
      Dim doc As Document = acApp.DocumentManager.MdiActiveDocument

      ltscale = doc.Database.Ltscale
      doc.Database.Ltscale = 0.01
      doc.Editor.Regen()

      doc.SendStringToExecute("_.HATCH ", False, False, False)

      AddHandler doc.CommandEnded, AddressOf doc_CommandEnded
      AddHandler doc.CommandCancelled, AddressOf doc_CommandEnded
      AddHandler doc.CommandFailed, AddressOf doc_CommandEnded
    End Sub

    Public Shared Sub doc_CommandEnded(sender As Object, e As Autodesk.AutoCAD.ApplicationServices.CommandEventArgs)
      If e.GlobalCommandName = "HATCH" Then
        Dim doc As Document = sender
        RemoveHandler doc.CommandEnded, AddressOf doc_CommandEnded
        RemoveHandler doc.CommandCancelled, AddressOf doc_CommandEnded
        RemoveHandler doc.CommandFailed, AddressOf doc_CommandEnded

        doc.Database.Ltscale = ltscale
        doc.Editor.Regen()
      End If
    End Sub
  End Class

End Namespace