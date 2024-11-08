namespace _my1;

public static class CardNumber
{
    public static string GetMnemonic(string cardNumber)
    {
        string paymentSystem;
        switch(cardNumber[0])
        {
            case '2':
                paymentSystem = "MIR";
                break;
            case '4':
                paymentSystem = "VISA";
                break;
            case '5':
                paymentSystem = "MASTERCARD";
                break;
            default:
                paymentSystem = "";
                break;
        }

        string last4 = cardNumber[^4..];
        string result = paymentSystem + " *" + last4;
        
        return result;
    }
}
