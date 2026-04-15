import re

with open('wwwroot/css/site.css', 'r', encoding='utf-8') as f:
    content = f.read()

pattern = r"\.auth-box\s*\{.*?(?=\/\* Homepage Center \*\/)"
new_css = """\
.auth-box {
    position: relative;
    display: inline-block;
    background-color: rgba(20, 20, 20, 0.65); /* Glassmorphism */
    backdrop-filter: blur(8px);
    color: white;
    text-decoration: none;
    padding: 12px 25px;
    border-radius: 12px;
    border: 1px solid rgba(255, 0, 0, 0.2);
    text-align: center;
    box-shadow: inset 0 0 12px rgba(102, 0, 0, 0.5), 0 0 15px rgba(0, 0, 0, 0.8);
    font-weight: bold;
    text-transform: uppercase;
    font-size: 16px;
    overflow: hidden;
    z-index: 0;
    transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.auth-box::before {
    content: "";
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: url('/images/asfalt-dark.png') repeat;
    opacity: 0.2; /* Initial texture opacity */
    pointer-events: none;
    border-radius: 12px;
    z-index: 1;
    transition: opacity 0.4s ease;
}

.auth-box:hover::before {
    opacity: 0.05; /* Lower = more transparent = background image shows */
}

.auth-box:hover {
    background-color: rgba(40, 10, 10, 0.85); /* Darker reddish black */
    border-color: rgba(255, 0, 0, 0.8);
    box-shadow: inset 0 0 20px rgba(204, 0, 0, 0.6), 0 0 30px rgba(255, 0, 0, 0.5);
    transform: scale(1.05); /* slightly scale */
    color: white;
}

"""

new_content = re.sub(pattern, new_css, content, count=1, flags=re.DOTALL)

with open('wwwroot/css/site.css', 'w', encoding='utf-8') as f:
    f.write(new_content)

print('Done!')
