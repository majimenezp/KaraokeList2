Imports System.IO
Public Class CdgFileIoStream

#Region "Declarations"

  Private m_file As Stream
#End Region

#Region "Public Methods"
  Public Sub New()
    m_file = Nothing
  End Sub

  Public Function read(ByRef buf As Byte(), ByVal buf_size As Integer) As Integer
    Return m_file.Read(buf, 0, buf_size)
  End Function

  Public Function write(ByRef buf As Byte(), ByVal buf_size As Integer) As Integer
    m_file.Write(buf, 0, buf_size)
    Return 1
  End Function

  Public Function seek(ByVal offset As Integer, ByVal whence As SeekOrigin) As Integer
    Return m_file.Seek(offset, whence)
  End Function

  Public Function eof() As Integer
    If (m_file.Position >= m_file.Length) Then
      Return 1
    Else
      Return 0
    End If
  End Function

  Public Function getsize() As Integer
    Return m_file.Length
  End Function

  Public Function open(ByVal filename As String) As Boolean
        close()
        Try
            m_file = New FileStream(filename, FileMode.Open)
        Catch ex As Exception
            Return False
        End Try

    Return (m_file IsNot Nothing)
  End Function

  Public Sub close()
    If (m_file IsNot Nothing) Then
      m_file.Close()
      m_file = Nothing
    End If
  End Sub

#End Region

End Class
