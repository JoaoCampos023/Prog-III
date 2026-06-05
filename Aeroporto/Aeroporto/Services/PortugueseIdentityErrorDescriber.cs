using Microsoft.AspNetCore.Identity;

namespace SistemaAereo.Services
{
    // Classe responsável por traduzir as mensagens de erro do Identity para português
    // Herda de IdentityErrorDescriber e sobrescreve os métodos com mensagens em português
    public class PortugueseIdentityErrorDescriber : IdentityErrorDescriber
    {
        // Erro de concorrência (dados modificados por outro usuário)
        public override IdentityError ConcurrencyFailure()
        {
            return new IdentityError
            {
                Code = nameof(ConcurrencyFailure),
                Description = "Falha de concorrência. Os dados foram modificados por outro usuário."
            };
        }

        // Erro padrão (inesperado)
        public override IdentityError DefaultError()
        {
            return new IdentityError
            {
                Code = nameof(DefaultError),
                Description = "Ocorreu um erro inesperado. Tente novamente."
            };
        }

        // Email já está em uso
        public override IdentityError DuplicateEmail(string email)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateEmail),
                Description = $"O email '{email}' já está sendo utilizado."
            };
        }

        // Nome da role já existe
        public override IdentityError DuplicateRoleName(string role)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateRoleName),
                Description = $"A função '{role}' já existe."
            };
        }

        // Nome de usuário já está em uso
        public override IdentityError DuplicateUserName(string userName)
        {
            return new IdentityError
            {
                Code = nameof(DuplicateUserName),
                Description = $"O nome de usuário '{userName}' já está sendo utilizado."
            };
        }

        // Email inválido
        public override IdentityError InvalidEmail(string email)
        {
            return new IdentityError
            {
                Code = nameof(InvalidEmail),
                Description = $"O email '{email}' é inválido."
            };
        }

        // Nome da role inválido
        public override IdentityError InvalidRoleName(string role)
        {
            return new IdentityError
            {
                Code = nameof(InvalidRoleName),
                Description = $"O nome da função '{role}' é inválido."
            };
        }

        // Token inválido
        public override IdentityError InvalidToken()
        {
            return new IdentityError
            {
                Code = nameof(InvalidToken),
                Description = "Token inválido."
            };
        }

        // Nome de usuário inválido
        public override IdentityError InvalidUserName(string userName)
        {
            return new IdentityError
            {
                Code = nameof(InvalidUserName),
                Description = $"O nome de usuário '{userName}' é inválido. Use apenas letras e números."
            };
        }

        // Login já está associado a outra conta
        public override IdentityError LoginAlreadyAssociated()
        {
            return new IdentityError
            {
                Code = nameof(LoginAlreadyAssociated),
                Description = "Este login já está associado a uma conta."
            };
        }

        // Senha incorreta
        public override IdentityError PasswordMismatch()
        {
            return new IdentityError
            {
                Code = nameof(PasswordMismatch),
                Description = "A senha está incorreta."
            };
        }

        // Senha precisa conter número
        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresDigit),
                Description = "A senha deve conter pelo menos um número ('0'-'9')."
            };
        }

        // Senha precisa conter letra minúscula
        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresLower),
                Description = "A senha deve conter pelo menos uma letra minúscula ('a'-'z')."
            };
        }

        // Senha precisa conter caractere especial
        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = "A senha deve conter pelo menos um caractere especial ('@', '#', '$', etc.)."
            };
        }

        // Senha precisa conter letra maiúscula
        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError
            {
                Code = nameof(PasswordRequiresUpper),
                Description = "A senha deve conter pelo menos uma letra maiúscula ('A'-'Z')."
            };
        }

        // Senha muito curta
        public override IdentityError PasswordTooShort(int length)
        {
            return new IdentityError
            {
                Code = nameof(PasswordTooShort),
                Description = $"A senha deve ter no mínimo {length} caracteres."
            };
        }

        // Falha na recuperação do código
        public override IdentityError RecoveryCodeRedemptionFailed()
        {
            return new IdentityError
            {
                Code = nameof(RecoveryCodeRedemptionFailed),
                Description = "Falha na recuperação do código."
            };
        }

        // Usuário já possui senha
        public override IdentityError UserAlreadyHasPassword()
        {
            return new IdentityError
            {
                Code = nameof(UserAlreadyHasPassword),
                Description = "O usuário já possui uma senha definida."
            };
        }

        // Usuário já pertence à role
        public override IdentityError UserAlreadyInRole(string role)
        {
            return new IdentityError
            {
                Code = nameof(UserAlreadyInRole),
                Description = $"O usuário já pertence à função '{role}'."
            };
        }

        // Bloqueio não está habilitado
        public override IdentityError UserLockoutNotEnabled()
        {
            return new IdentityError
            {
                Code = nameof(UserLockoutNotEnabled),
                Description = "O bloqueio não está habilitado para este usuário."
            };
        }

        // Usuário não pertence à role
        public override IdentityError UserNotInRole(string role)
        {
            return new IdentityError
            {
                Code = nameof(UserNotInRole),
                Description = $"O usuário não pertence à função '{role}'."
            };
        }
    }
}