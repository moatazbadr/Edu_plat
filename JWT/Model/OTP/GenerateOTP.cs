namespace JWT.Model.OTP
{
    public static class GenerateOTP
    {
        public static string GenerateOtp()
        {
            
            Random rand = new Random();
		   
            var RandomOtp = rand.Next(10000, 100000).ToString();  
            return RandomOtp;
        }

    }
}
