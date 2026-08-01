namespace BertBridge.Domain.Fec;

/// <summary>
/// FEC 类型枚举。
/// </summary>
public enum FecType
{
    /// <summary>无 FEC</summary>
    None,

    /// <summary>KR4: RS(528,514,t=7,m=10)，100G NRZ</summary>
    KR4,

    /// <summary>KP4: RS(544,514,t=15,m=10)，400G/800G PAM4</summary>
    KP4,

    /// <summary>FC-FEC: Firecode (2112,2080)，16G/32G FC</summary>
    FC,

    /// <summary>级联 FEC</summary>
    Concatenated,

    /// <summary>LDPC</summary>
    LDPC
}
