// CEP Auto Complete
function buscarCep() {
    const cepInput = document.getElementById('Cep');
    let cep = cepInput.value.replace(/\D/g, '');

    if (cep.length !== 8) {
        limparCamposEndereco();
        return;
    }

    mostrarLoadingCep(true);

    fetch(`/api/cep/${cep}`)
        .then(response => response.json())
        .then(data => {
            if (data.success && data.data) {
                preencherCamposEndereco(data.data);
                mostrarMensagemSucesso('CEP encontrado!');
            } else {
                limparCamposEndereco();
                mostrarMensagemErro('CEP não encontrado');
            }
        })
        .catch(error => {
            console.error('Erro:', error);
            limparCamposEndereco();
            mostrarMensagemErro('Erro ao buscar CEP');
        })
        .finally(() => {
            mostrarLoadingCep(false);
        });
}

function preencherCamposEndereco(endereco) {
    const logradouroInput = document.getElementById('Endereco');
    const bairroInput = document.getElementById('Bairro');
    const cidadeInput = document.getElementById('Cidade');
    const ufSelect = document.getElementById('Estado');

    if (logradouroInput) {
        const logradouroCompleto = endereco.logradouro;
        if (endereco.complemento) {
            logradouroInput.value = `${logradouroCompleto} - ${endereco.complemento}`;
        } else {
            logradouroInput.value = logradouroCompleto;
        }
    }

    if (bairroInput) bairroInput.value = endereco.bairro || '';
    if (cidadeInput) cidadeInput.value = endereco.cidade || '';
    
    if (ufSelect && endereco.uf) {
        for (let i = 0; i < ufSelect.options.length; i++) {
            if (ufSelect.options[i].value === endereco.uf) {
                ufSelect.selectedIndex = i;
                break;
            }
        }
    }
}

function limparCamposEndereco() {
    const logradouroInput = document.getElementById('Endereco');
    const bairroInput = document.getElementById('Bairro');
    const cidadeInput = document.getElementById('Cidade');

    if (logradouroInput && logradouroInput.value === '') return;
    
    if (logradouroInput) logradouroInput.value = '';
    if (bairroInput) bairroInput.value = '';
    if (cidadeInput) cidadeInput.value = '';
}

function mostrarLoadingCep(show) {
    const loadingElement = document.getElementById('cepLoading');
    if (loadingElement) {
        loadingElement.style.display = show ? 'inline-block' : 'none';
    }
}

function mostrarMensagemSucesso(mensagem) {
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-success alert-dismissible fade show position-fixed top-0 end-0 m-3';
    alertDiv.style.zIndex = '9999';
    alertDiv.innerHTML = `
        <i class="fas fa-check-circle me-2"></i>
        ${mensagem}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.appendChild(alertDiv);
    setTimeout(() => alertDiv.remove(), 3000);
}

function mostrarMensagemErro(mensagem) {
    const alertDiv = document.createElement('div');
    alertDiv.className = 'alert alert-danger alert-dismissible fade show position-fixed top-0 end-0 m-3';
    alertDiv.style.zIndex = '9999';
   alertDiv.innerHTML = `
        <i class="fas fa-exclamation-circle me-2"></i>
        ${mensagem}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    document.body.appendChild(alertDiv);
    setTimeout(() => alertDiv.remove(), 3000);
}

// Máscara para CEP
function aplicarMascaraCep() {
    const cepInput = document.getElementById('Cep');
    if (cepInput) {
        cepInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length > 5) {
                value = value.replace(/^(\d{5})(\d)/, '$1-$2');
            }
            e.target.value = value;
        });
    }
}

// Inicializar quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function() {
    aplicarMascaraCep();
    
    const cepInput = document.getElementById('Cep');
    if (cepInput) {
        cepInput.addEventListener('blur', buscarCep);
    }
});