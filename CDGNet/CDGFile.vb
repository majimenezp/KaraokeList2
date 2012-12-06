Imports System.IO
Imports System.Drawing
Imports System.Runtime.InteropServices


Public Class CDGFile
  Implements IDisposable


#Region "Constants"
  'CDG Command Code
  Const CDG_COMMAND As Byte = &H9

  'CDG Instruction Codes
  Const CDG_INST_MEMORY_PRESET As Integer = 1
  Const CDG_INST_BORDER_PRESET As Integer = 2
  Const CDG_INST_TILE_BLOCK As Integer = 6
  Const CDG_INST_SCROLL_PRESET As Integer = 20
  Const CDG_INST_SCROLL_COPY As Integer = 24
  Const CDG_INST_DEF_TRANSP_COL As Integer = 28
  Const CDG_INST_LOAD_COL_TBL_LO As Integer = 30
  Const CDG_INST_LOAD_COL_TBL_HIGH As Integer = 31
  Const CDG_INST_TILE_BLOCK_XOR As Integer = 38

  'Bitmask for all CDG fields
  Const CDG_MASK As Byte = &H3F
  Const CDG_PACKET_SIZE As Integer = 24
  Const TILE_HEIGHT As Integer = 12
  Const TILE_WIDTH As Integer = 6

  'This is the size of the display as defined by the CDG specification.
  'The pixels in this region can be painted, and scrolling operations
  'rotate through this number of pixels.
  Public Const CDG_FULL_WIDTH As Integer = 300
  Public Const CDG_FULL_HEIGHT As Integer = 216

  'This is the size of the screen that is actually intended to be
  'visible.  It is the center area of CDG_FULL.  
  Const CDG_DISPLAY_WIDTH As Integer = 294
  Const CDG_DISPLAY_HEIGHT As Integer = 204

  Const COLOUR_TABLE_SIZE As Integer = 16
#End Region

#Region "Private Declarations"

  Private m_pixelColours(CDG_FULL_HEIGHT - 1, CDG_FULL_WIDTH - 1) As Byte
  Private m_colourTable(COLOUR_TABLE_SIZE - 1) As Integer
  Private m_presetColourIndex As Integer
  Private m_borderColourIndex As Integer
  Private m_transparentColour As Integer

  Private m_hOffset As Integer
  Private m_vOffset As Integer

  Private m_pStream As CdgFileIoStream
  Private m_pSurface As ISurface
  Private m_positionMs As Long
  Private m_duration As Long

  Private mImage As Bitmap

#End Region

#Region "Properties"

  Public ReadOnly Property RGBImage(Optional ByVal makeTransparent As Boolean = False) As System.Drawing.Image
    Get
      Dim temp As New MemoryStream
      Try
        Dim i As Integer = 0
        For ri = 0 To CDG_FULL_HEIGHT - 1
          For ci = 0 To CDG_FULL_WIDTH - 1
            Dim ARGBInt As Integer = m_pSurface.rgbData(ri, ci)
            Dim myByte(3) As Byte
            myByte = BitConverter.GetBytes(ARGBInt)
            temp.Write(myByte, 0, 4)
          Next
        Next
      Catch ex As Exception
        'Do nothing (empty bitmap will be returned)
      End Try
      Dim myBitmap As Bitmap = GraphicUtil.StreamToBitmap(temp, CDG_FULL_WIDTH, CDG_FULL_HEIGHT)
      If makeTransparent Then
        myBitmap.MakeTransparent(myBitmap.GetPixel(1, 1))
      End If
            Return myBitmap
    End Get

  End Property

#End Region

#Region "Public Methods"

  'Png Export
  Public Sub SavePng(ByVal filename As String)
    RGBImage.Save(filename, System.Drawing.Imaging.ImageFormat.Png)
  End Sub

  'New
  Public Sub New(ByVal cdgFileName As String)
    m_pStream = New CdgFileIoStream
    m_pStream.open(cdgFileName)
    m_pSurface = New ISurface
    If m_pStream IsNot Nothing AndAlso m_pSurface IsNot Nothing Then
      Me.reset()
      m_duration = ((m_pStream.getsize() / CDG_PACKET_SIZE) * 1000) / 300
    End If
  End Sub

  Public Function getTotalDuration() As Long
    Return m_duration
  End Function

  Public Function renderAtPosition(ByVal ms As Long) As Boolean
    Dim pack As New CdgPacket
    Dim numPacks As Long = 0
    Dim res As Boolean = True

    If (m_pStream Is Nothing) Then
      Return False
    End If

    If (ms < m_positionMs) Then
      If (m_pStream.seek(0, SeekOrigin.Begin) < 0) Then Return False
      m_positionMs = 0
    End If

    'duration of one packet is 1/300 seconds (4 packets per sector, 75 sectors per second)

    numPacks = ms - m_positionMs
    numPacks /= 10

    m_positionMs += numPacks * 10
    numPacks *= 3

    'TODO: double check logic due to inline while loop fucntionality
    While numPacks > 0 'AndAlso m_pSurface.rgbData Is Nothing
      res = readPacket(pack)
      processPacket(pack)
      numPacks -= 1
    End While

    render()
    Return res

  End Function

#End Region

#Region "Private Methods"

  Private Sub reset()

    Array.Clear(m_pixelColours, 0, m_pixelColours.LongLength)
    Array.Clear(m_colourTable, 0, m_colourTable.LongLength)

    m_presetColourIndex = 0
    m_borderColourIndex = 0
    m_transparentColour = 0
    m_hOffset = 0
    m_vOffset = 0

    m_duration = 0
    m_positionMs = 0

    'clear surface 
    If (m_pSurface.rgbData IsNot Nothing) Then
      Array.Clear(m_pSurface.rgbData, 0, m_pSurface.rgbData.LongLength)
    End If

  End Sub

  Private Function readPacket(ByRef pack As CdgPacket) As Boolean

    If m_pStream Is Nothing Or m_pStream.eof() Then
      Return False
    End If

    Dim read As Integer = 0

    read += m_pStream.read(pack.command, 1)
    read += m_pStream.read(pack.instruction, 1)
    read += m_pStream.read(pack.parityQ, 2)
    read += m_pStream.read(pack.data, 16)
    read += m_pStream.read(pack.parityP, 4)

    Return (read = 24)
  End Function

  Private Sub processPacket(ByRef pack As CdgPacket)

    Dim inst_code As Integer

    If ((pack.command(0) And CDG_MASK) = CDG_COMMAND) Then
      inst_code = (pack.instruction(0) And CDG_MASK)
      Select Case inst_code
        Case CDG_INST_MEMORY_PRESET
          memoryPreset(pack)
          Exit Sub

        Case CDG_INST_BORDER_PRESET
          borderPreset(pack)
          Exit Sub

        Case CDG_INST_TILE_BLOCK
          tileBlock(pack, False)
          Exit Sub

        Case CDG_INST_SCROLL_PRESET
          scroll(pack, False)
          Exit Sub

        Case CDG_INST_SCROLL_COPY
          scroll(pack, True)
          Exit Sub

        Case CDG_INST_DEF_TRANSP_COL
          defineTransparentColour(pack)
          Exit Sub

        Case CDG_INST_LOAD_COL_TBL_LO
          loadColorTable(pack, 0)
          Exit Sub

        Case CDG_INST_LOAD_COL_TBL_HIGH
          loadColorTable(pack, 1)
          Exit Sub

        Case CDG_INST_TILE_BLOCK_XOR
          tileBlock(pack, True)
          Exit Sub

        Case Else
          'Ignore the unsupported commands
          Exit Sub
      End Select
    End If
  End Sub

  Private Sub memoryPreset(ByRef pack As CdgPacket)

    Dim colour As Integer
    Dim ri As Integer
    Dim ci As Integer
    Dim repeat As Integer

    colour = pack.data(0) And &HF
    repeat = pack.data(1) And &HF

    'Our new interpretation of CD+G Revealed is that memory preset
    'commands should also change the border
    m_presetColourIndex = colour
    m_borderColourIndex = colour

    'we have a reliable data stream, so the repeat command 
    'is executed only the first time

    If (repeat = 0) Then

      'Note that this may be done before any load colour table
      'commands by some CDGs. So the load colour table itself
      'actual recalculates the RGB values for all pixels when
      'the colour table changes.

      'Set the preset colour for every pixel. Must be stored in 
      'the pixel colour table indeces array

      For ri = 0 To CDG_FULL_HEIGHT - 1
        For ci = 0 To CDG_FULL_WIDTH - 1
          m_pixelColours(ri, ci) = colour
        Next
      Next
    End If

  End Sub

  Private Sub borderPreset(ByRef pack As CdgPacket)

    Dim colour As Integer
    Dim ri As Integer
    Dim ci As Integer

    colour = pack.data(0) And &HF
    m_borderColourIndex = colour

    'The border area is the area contained with a rectangle 
    'defined by (0,0,300,216) minus the interior pixels which are contained
    'within a rectangle defined by (6,12,294,204).

    For ri = 0 To CDG_FULL_HEIGHT - 1
      For ci = 0 To 5
        m_pixelColours(ri, ci) = colour
      Next

      For ci = CDG_FULL_WIDTH - 6 To CDG_FULL_WIDTH - 1
        m_pixelColours(ri, ci) = colour
      Next
    Next

    For ci = 6 To CDG_FULL_WIDTH - 7
      For ri = 0 To 11
        m_pixelColours(ri, ci) = colour
      Next

      For ri = CDG_FULL_HEIGHT - 12 To CDG_FULL_HEIGHT - 1
        m_pixelColours(ri, ci) = colour
      Next
    Next

  End Sub

  Private Sub loadColorTable(ByRef pack As CdgPacket, ByVal table As Integer)

    For i As Integer = 0 To 7

      '[---high byte---]   [---low byte----]
      '7 6 5 4 3 2 1 0     7 6 5 4 3 2 1 0
      'X X r r r r g g     X X g g b b b b

      Dim byte0 As Byte = pack.data(2 * i)
      Dim byte1 As Byte = pack.data(2 * i + 1)
      Dim red As Integer = (byte0 And &H3F) >> 2
      Dim green As Integer = ((byte0 And &H3) << 2) Or ((byte1 And &H3F) >> 4)
      Dim blue As Integer = byte1 And &HF

      red *= 17
      green *= 17
      blue *= 17

      If m_pSurface IsNot Nothing Then
        m_colourTable(i + table * 8) = m_pSurface.MapRGBColour(red, green, blue)
      End If
    Next

  End Sub

  Private Sub tileBlock(ByRef pack As CdgPacket, ByVal bXor As Boolean)

    Dim colour0 As Integer
    Dim colour1 As Integer
    Dim column_index As Integer
    Dim row_index As Integer
    Dim myByte As Integer
    Dim pixel As Integer
    Dim xor_col As Integer
    Dim currentColourIndex As Integer
    Dim new_col As Integer

    colour0 = pack.data(0) And &HF
    colour1 = pack.data(1) And &HF
    row_index = ((pack.data(2) And &H1F) * 12)
    column_index = ((pack.data(3) And &H3F) * 6)

    If (row_index > (CDG_FULL_HEIGHT - TILE_HEIGHT)) Then Exit Sub
    If (column_index > (CDG_FULL_WIDTH - TILE_WIDTH)) Then Exit Sub

    'Set the pixel array for each of the pixels in the 12x6 tile.
    'Normal = Set the colour to either colour0 or colour1 depending
    'on whether the pixel value is 0 or 1.
    'XOR = XOR the colour with the colour index currently there.

    For i As Integer = 0 To 11

      myByte = (pack.data(4 + i) And &H3F)
      For j As Integer = 0 To 5
        pixel = (myByte >> (5 - j)) And &H1
        If (bXor) Then
          'Tile Block XOR 
          If (pixel = 0) Then
            xor_col = colour0
          Else
            xor_col = colour1
          End If

          'Get the colour index currently at this location, and xor with it 
          currentColourIndex = m_pixelColours(row_index + i, column_index + j)
          new_col = currentColourIndex Xor xor_col
        Else
          If (pixel = 0) Then
            new_col = colour0
          Else
            new_col = colour1
          End If
        End If

        'Set the pixel with the new colour. We set both the surfarray
        'containing actual RGB values, as well as our array containing
        'the colour indexes into our colour table. 
        m_pixelColours(row_index + i, column_index + j) = new_col
      Next

    Next
  End Sub

  Private Sub defineTransparentColour(ByRef pack As CdgPacket)
    m_transparentColour = pack.data(0) And &HF
  End Sub

  Private Sub scroll(ByRef pack As CdgPacket, ByVal copy As Boolean)

    Dim colour As Integer
    Dim hScroll As Integer
    Dim vScroll As Integer
    Dim hSCmd As Integer
    Dim hOffset As Integer
    Dim vSCmd As Integer
    Dim vOffset As Integer
    Dim vScrollPixels As Integer
    Dim hScrollPixels As Integer

    'Decode the scroll command parameters
    colour = pack.data(0) And &HF
    hScroll = pack.data(1) And &H3F
    vScroll = pack.data(2) And &H3F

    hSCmd = (hScroll And &H30) >> 4
    hOffset = (hScroll And &H7)
    vSCmd = (vScroll And &H30) >> 4
    vOffset = (vScroll And &HF)


    m_hOffset = If(hOffset < 5, hOffset, 5)
    m_vOffset = If(vOffset < 11, vOffset, 11)

    'Scroll Vertical - Calculate number of pixels

    vScrollPixels = 0
    If (vSCmd = 2) Then
      vScrollPixels = -12
    ElseIf (vSCmd = 1) Then
      vScrollPixels = 12
    End If

    'Scroll Horizontal- Calculate number of pixels

    hScrollPixels = 0
    If (hSCmd = 2) Then
      hScrollPixels = -6
    ElseIf (hSCmd = 1) Then
      hScrollPixels = 6
    End If

    If (hScrollPixels = 0 AndAlso vScrollPixels = 0) Then
      Exit Sub
    End If

    'Perform the actual scroll.

    Dim temp(CDG_FULL_HEIGHT, CDG_FULL_WIDTH) As Byte
    Dim vInc As Integer = vScrollPixels + CDG_FULL_HEIGHT
    Dim hInc As Integer = hScrollPixels + CDG_FULL_WIDTH
    Dim ri As Integer 'row index
    Dim ci As Integer 'column index

    For ri = 0 To CDG_FULL_HEIGHT - 1
      For ci = 0 To CDG_FULL_WIDTH - 1
        temp((ri + vInc) Mod CDG_FULL_HEIGHT, (ci + hInc) Mod CDG_FULL_WIDTH) = m_pixelColours(ri, ci)
      Next
    Next


    'if copy is false, we were supposed to fill in the new pixels
    'with a new colour. Go back and do that now.

    If (copy = False) Then

      If (vScrollPixels > 0) Then

        For ci = 0 To CDG_FULL_WIDTH - 1
          For ri = 0 To vScrollPixels - 1
            temp(ri, ci) = colour
          Next
        Next

      ElseIf (vScrollPixels < 0) Then

        For ci = 0 To CDG_FULL_WIDTH - 1
          For ri = CDG_FULL_HEIGHT + vScrollPixels To CDG_FULL_HEIGHT - 1
            temp(ri, ci) = colour
          Next
        Next

      End If

      If (hScrollPixels > 0) Then

        For ci = 0 To hScrollPixels - 1
          For ri = 0 To CDG_FULL_HEIGHT - 1
            temp(ri, ci) = colour
          Next
        Next

      ElseIf (hScrollPixels < 0) Then

        For ci = CDG_FULL_WIDTH + hScrollPixels To CDG_FULL_WIDTH - 1
          For ri = 0 To CDG_FULL_HEIGHT - 1
            temp(ri, ci) = colour
          Next
        Next

      End If

    End If

    'Now copy the temporary buffer back to our array

    For ri = 0 To CDG_FULL_HEIGHT - 1
      For ci = 0 To CDG_FULL_WIDTH - 1
        m_pixelColours(ri, ci) = temp(ri, ci)
      Next
    Next

  End Sub

  Private Sub render()

    If (m_pSurface Is Nothing) Then Exit Sub
    For ri As Integer = 0 To CDG_FULL_HEIGHT - 1
      For ci As Integer = 0 To CDG_FULL_WIDTH - 1
        If (ri < TILE_HEIGHT OrElse ri >= CDG_FULL_HEIGHT - TILE_HEIGHT _
            OrElse ci < TILE_WIDTH OrElse ci >= CDG_FULL_WIDTH - TILE_WIDTH) Then
          m_pSurface.rgbData(ri, ci) = m_colourTable(m_borderColourIndex)
        Else
          m_pSurface.rgbData(ri, ci) = m_colourTable(m_pixelColours(ri + m_vOffset, ci + m_hOffset))
        End If
      Next
    Next

  End Sub

#End Region

  Private disposedValue As Boolean = False    ' To detect redundant calls

  ' IDisposable
  Protected Overridable Sub Dispose(ByVal disposing As Boolean)
    If Not Me.disposedValue Then
      If disposing Then
        m_pStream.close()
      End If
      m_pStream = Nothing
      m_pSurface = Nothing
    End If
    Me.disposedValue = True
  End Sub

#Region " IDisposable Support "
  ' This code added by Visual Basic to correctly implement the disposable pattern.
  Public Sub Dispose() Implements IDisposable.Dispose
    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    Dispose(True)
    GC.SuppressFinalize(Me)
  End Sub
#End Region

End Class

Public Class CdgPacket
  Public command(0) As Byte
  Public instruction(0) As Byte
  Public parityQ(1) As Byte
  Public data(15) As Byte
  Public parityP(3) As Byte
End Class

Public Class ISurface

  Public rgbData(CDGFile.CDG_FULL_HEIGHT - 1, CDGFile.CDG_FULL_WIDTH - 1) As Long

  Public Function MapRGBColour(ByVal red As Integer, ByVal green As Integer, ByVal blue As Integer) As Integer
    Return Color.FromArgb(red, green, blue).ToArgb
  End Function

End Class


