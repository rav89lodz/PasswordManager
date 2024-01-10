using PasswordManager.Interfaces;
using System;

namespace PasswordManager.Services
{
    public class PasswordService : IPasswordService
    {
        private Random random = new Random();

        public PasswordService() { }

        public char RandomLowerLetter()
        {
            int num = random.Next(0, 26);
            char let = (char)('a' + num);
            return let;
        }

        public char RandomUpperLetter()
        {
            int num = random.Next(0, 26);
            char let = (char)('A' + num);
            return let;
        }

        public string CreateRandomString(int iterations, bool withChars)
        {
            string randomString = "";
            char[] charset = new char[7] { '$', '%', '@', '#', '&', '[', ']' };

            for (int i = 0; i < random.Next(2, 4) + iterations; i ++)
            {
                int draw = random.Next(3, 99);
                int randomNumber = random.Next(0, 6);

                if (draw % 2 == 0)
                {
                    randomString += RandomLowerLetter();
                }
                else
                {
                    randomString += RandomUpperLetter();
                }

                if(draw > 29 && draw < 74)
                {
                    randomString += Convert.ToString(random.Next(5, 10));
                }
                else
                {
                    if (withChars == true)
                    {
                        int randomChars = random.Next(1, 99);
                        if (randomChars > 50)
                        {
                            randomString += Convert.ToString(charset[randomNumber]);
                        }
                        else if (randomChars > 22 && randomChars <= 50)
                        {
                            randomString += Convert.ToString(random.Next(5, 10));
                        }
                        else
                        {
                            randomString += RandomUpperLetter();
                        }
                    }
                    else
                    {
                        randomString += RandomUpperLetter();
                    }
                }

                if(randomNumber > 3)
                {
                    randomString += Convert.ToString(random.Next(0, 6));
                }
                else
                {
                    randomString += RandomLowerLetter();
                }
            }
            return randomString;
        }

        public string PassGen(int option)
        {
            if (option == 0) // strong
            {
                return CreateRandomString(0, true);
            }
            if (option == 1) // very strong
            {
                return CreateRandomString(1, true);
            }
            return CreateRandomString(3, true); // extremely strong 
        }
    }
}
