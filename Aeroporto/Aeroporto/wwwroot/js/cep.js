// CEP Auto Complete
function buscarCep() {
    // CORRIGIDO: Usando ZipCode (padrão do Customer)
    const cepInput = document.getElementById('ZipCode');
    if (!cepInput) {
        console.log('Campo CEP não encontrado');
        return;
    }

    let cep = cepInput.value.replace(/\D/g, '');

    if (cep.length !== 8) {
        limparCamposEndereco();
        return;
    }

    console.log('Buscando CEP:', cep);
    mostrarLoadingCep(true);

    fetch(`/api/cep/${cep}`)
        .then(response => response.json())
        .then(data => {
            console.log('Resposta da API:', data);
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
    // CORRIGIDO: Usando Address, City, State (padrão do Customer)
    const addressInput = document.getElementById('Address');
    const cityInput = document.getElementById('City');
    const stateSelect = document.getElementById('State');

    console.log('Preenchendo campos:', endereco);

    if (addressInput) {
        let enderecoCompleto = endereco.street || endereco.logradouro || '';
        if (endereco.complement || endereco.complemento) {
            enderecoCompleto += ` - ${endereco.complement || endereco.complemento}`;
        }
        addressInput.value = enderecoCompleto;
    }

    if (cityInput) {
        cityInput.value = endereco.city || endereco.cidade || '';
    }

    if (stateSelect && (endereco.state || endereco.uf)) {
        const estado = endereco.state || endereco.uf;
        for (let i = 0; i < stateSelect.options.length; i++) {
            if (stateSelect.options[i].value === estado) {
                stateSelect.selectedIndex = i;
                break;
            }
        }
    }
}

function limparCamposEndereco() {
    const addressInput = document.getElementById('Address');
    const cityInput = document.getElementById('City');
    const stateSelect = document.getElementById('State');

    if (addressInput && addressInput.value === '') return;

    if (addressInput) addressInput.value = '';
    if (cityInput) cityInput.value = '';
    if (stateSelect) stateSelect.selectedIndex = 0;
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
    const cepInput = document.getElementById('ZipCode');
    if (cepInput) {
        cepInput.addEventListener('input', function (e) {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length > 5) {
                value = value.replace(/^(\d{5})(\d)/, '$1-$2');
            }
            e.target.value = value;
        });
    }
}

// Inicializar quando o DOM estiver pronto
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM carregado - Inicializando CEP');
    aplicarMascaraCep();

    const cepInput = document.getElementById('ZipCode');
    if (cepInput) {
        console.log('Campo CEP encontrado');
        cepInput.addEventListener('blur', buscarCep);
        cepInput.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                buscarCep();
            }
        });
    } else {
        console.log('Campo CEP NÃO encontrado! O ID deve ser "ZipCode"');
    }
});